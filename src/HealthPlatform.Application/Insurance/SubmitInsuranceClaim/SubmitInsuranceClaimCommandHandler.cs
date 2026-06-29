using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Insurance;
using MediatR;

namespace HealthPlatform.Application.Insurance.SubmitInsuranceClaim;

public sealed class SubmitInsuranceClaimCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IPatientInsurancePolicyRepository policyRepository,
    IInsuranceClaimRepository claimRepository,
    IInsurerApiClientResolver insurerApiClientResolver,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<SubmitInsuranceClaimCommand, InsuranceClaimDto>
{
    public async Task<InsuranceClaimDto> Handle(SubmitInsuranceClaimCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var asOfDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var insurerCode = request.InsurerCode.Trim().ToLowerInvariant();

        var policy = await policyRepository.GetActiveByPatientAndInsurerAsync(
            patient.Id,
            insurerCode,
            asOfDate,
            ct);

        if (policy is null)
        {
            throw new DomainException(
                InsuranceErrorCodes.PolicyInactive,
                "Patient does not have an active insurance policy for the requested insurer.");
        }

        if (await claimRepository.ExistsForTargetAsync(
                patient.Id,
                request.ClaimType,
                request.AppointmentId,
                request.MedicationOrderId,
                request.LabOrderId,
                ct))
        {
            throw new ConflictException(
                InsuranceErrorCodes.DuplicateClaim,
                "An insurance claim already exists for this service.");
        }

        var claim = InsuranceClaim.Create(
            patient.Id,
            policy.Id,
            insurerCode,
            request.ClaimType,
            request.AmountMinorUnits,
            request.Currency,
            request.AppointmentId,
            request.MedicationOrderId,
            request.LabOrderId);

        var insurerClient = insurerApiClientResolver.GetRequired(insurerCode);
        var submission = await insurerClient.SubmitClaimAsync(
            new InsurerClaimSubmissionRequest(
                claim.Id,
                patient.Id,
                policy.PolicyNumber,
                policy.MemberNumber,
                claim.ClaimType,
                claim.AmountMinorUnits,
                claim.Currency,
                claim.AppointmentId,
                claim.MedicationOrderId,
                claim.LabOrderId),
            ct);

        if (!submission.Succeeded || string.IsNullOrWhiteSpace(submission.InsurerClaimReference))
        {
            throw new DomainException(
                submission.FailureCode ?? InsuranceErrorCodes.InsurerUnavailable,
                submission.FailureMessage ?? "The insurer could not accept the claim.");
        }

        claim.MarkSubmitted(submission.InsurerClaimReference, timeProvider.GetUtcNow().UtcDateTime);
        await claimRepository.AddAsync(claim, ct);

        foreach (var domainEvent in claim.DomainEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        claim.ClearDomainEvents();
        await claimRepository.SaveChangesAsync(ct);

        return claim.ToDto();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");
    }
}
