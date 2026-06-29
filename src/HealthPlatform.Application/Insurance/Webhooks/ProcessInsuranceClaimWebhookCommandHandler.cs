using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Insurance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Insurance.Webhooks;

public sealed class ProcessInsuranceClaimWebhookCommandHandler(
    IInsurerApiClientResolver insurerApiClientResolver,
    IInsuranceClaimRepository claimRepository,
    IInsuranceClaimWebhookIdempotencyStore idempotencyStore,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider,
    ILogger<ProcessInsuranceClaimWebhookCommandHandler> logger)
    : IRequestHandler<ProcessInsuranceClaimWebhookCommand, ProcessInsuranceClaimWebhookResultDto>
{
    public async Task<ProcessInsuranceClaimWebhookResultDto> Handle(
        ProcessInsuranceClaimWebhookCommand request,
        CancellationToken ct)
    {
        var insurerCode = request.InsurerCode.Trim().ToLowerInvariant();
        var insurerClient = insurerApiClientResolver.GetRequired(insurerCode);
        var parsed = await insurerClient.ParseStatusWebhookAsync(
            new InsurerWebhookParseRequest(request.RawBody, request.Headers),
            ct);

        if (!parsed.SignatureValid)
        {
            throw new AccessDeniedException(
                "WEBHOOK_SIGNATURE_INVALID",
                "Insurance claim webhook signature validation failed.");
        }

        if (parsed.Status is null
            || string.IsNullOrWhiteSpace(parsed.InsurerClaimReference)
            || string.IsNullOrWhiteSpace(parsed.EventId))
        {
            return new ProcessInsuranceClaimWebhookResultDto(true, false, parsed.Status);
        }

        if (!await idempotencyStore.TryBeginProcessingAsync(insurerCode, parsed.EventId, ct))
        {
            logger.LogInformation(
                "Ignoring duplicate insurance webhook {InsurerCode} event {EventId}.",
                insurerCode,
                parsed.EventId);

            return new ProcessInsuranceClaimWebhookResultDto(true, true, parsed.Status);
        }

        var claim = await claimRepository.GetByInsurerReferenceAsync(
            insurerCode,
            parsed.InsurerClaimReference,
            ct);

        if (claim is null)
        {
            logger.LogWarning(
                "Insurance webhook referenced unknown claim {InsurerClaimReference} for insurer {InsurerCode}.",
                parsed.InsurerClaimReference,
                insurerCode);

            return new ProcessInsuranceClaimWebhookResultDto(true, false, parsed.Status);
        }

        var changed = claim.TryUpdateStatus(
            parsed.Status.Value,
            parsed.StatusReason,
            timeProvider.GetUtcNow().UtcDateTime);

        if (changed)
        {
            await claimRepository.UpdateAsync(claim, ct);
            await PublishDomainEventsAsync(claim, ct);
            await claimRepository.SaveChangesAsync(ct);
        }
        else
        {
            claim.RecordStatusCheck(timeProvider.GetUtcNow().UtcDateTime);
            await claimRepository.UpdateAsync(claim, ct);
            await claimRepository.SaveChangesAsync(ct);
        }

        return new ProcessInsuranceClaimWebhookResultDto(true, false, parsed.Status);
    }

    private async Task PublishDomainEventsAsync(InsuranceClaim claim, CancellationToken ct)
    {
        foreach (var domainEvent in claim.DomainEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        claim.ClearDomainEvents();
    }
}
