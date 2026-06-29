using HealthPlatform.Application.Insurance;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Insurance;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Insurance;

public sealed class InsuranceClaimStatusPoller(
    IInsuranceClaimRepository claimRepository,
    IInsurerApiClientResolver insurerApiClientResolver,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider,
    ILogger<InsuranceClaimStatusPoller> logger) : IInsuranceClaimStatusPoller
{
    private const int BatchSize = 50;
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(15);

    public async Task<int> PollPendingClaimsAsync(CancellationToken ct)
    {
        var checkedBeforeUtc = timeProvider.GetUtcNow().UtcDateTime.Subtract(PollInterval);
        var claims = await claimRepository.ListPendingStatusChecksAsync(checkedBeforeUtc, BatchSize, ct);
        var updated = 0;

        foreach (var claim in claims)
        {
            if (string.IsNullOrWhiteSpace(claim.InsurerClaimReference))
            {
                continue;
            }

            var insurerClient = insurerApiClientResolver.GetRequired(claim.InsurerCode);
            var statusResult = await insurerClient.GetClaimStatusAsync(claim.InsurerClaimReference, ct);
            if (!statusResult.Succeeded || statusResult.Status is null)
            {
                claim.RecordStatusCheck(timeProvider.GetUtcNow().UtcDateTime);
                await claimRepository.UpdateAsync(claim, ct);
                continue;
            }

            var changed = claim.TryUpdateStatus(
                statusResult.Status.Value,
                statusResult.StatusReason,
                timeProvider.GetUtcNow().UtcDateTime);

            if (changed)
            {
                updated++;
                await claimRepository.UpdateAsync(claim, ct);
                await PublishDomainEventsAsync(claim, ct);
            }
            else
            {
                claim.RecordStatusCheck(timeProvider.GetUtcNow().UtcDateTime);
                await claimRepository.UpdateAsync(claim, ct);
            }
        }

        if (updated > 0)
        {
            logger.LogInformation("Insurance claim status polling updated {Count} claim(s).", updated);
        }

        await claimRepository.SaveChangesAsync(ct);
        return updated;
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
