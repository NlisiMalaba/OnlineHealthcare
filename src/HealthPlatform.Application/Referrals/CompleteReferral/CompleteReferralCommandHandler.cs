using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Referrals;
using MediatR;

namespace HealthPlatform.Application.Referrals.CompleteReferral;

public sealed class CompleteReferralCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    IReferralRepository referralRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<CompleteReferralCommand, ReferralDto>
{
    public async Task<ReferralDto> Handle(CompleteReferralCommand request, CancellationToken ct)
    {
        var receivingDoctor = await ResolveDoctorAsync(ct);
        var referral = await referralRepository.GetByIdAsync(request.ReferralId, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.ReferralNotFound,
                "Referral was not found.");

        if (!referral.ReceivingDoctorId.HasValue || referral.ReceivingDoctorId.Value != receivingDoctor.Id)
        {
            throw new AccessDeniedException(
                ReferralErrorCodes.ReferralAccessDenied,
                "Only the receiving doctor can complete this referral.");
        }

        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(referral.PatientId, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.HealthRecordNotFound,
                "Patient health record was not found.");

        var completedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var summaryEntry = new HealthRecordReferralConsultationSummaryEntry(
            healthRecord.Id,
            referral.PatientId,
            receivingDoctor.Id,
            referral.Id,
            request.ConsultationSummary.Trim(),
            completedAtUtc);

        var summaryReference = await healthRecordEntryRepository.AddReferralConsultationSummaryEntryAsync(summaryEntry, ct);

        try
        {
            referral.Complete(summaryReference.EntryDocumentId, completedAtUtc);
        }
        catch (ReferralCompletionNotAllowedException ex)
        {
            throw new DomainException(ReferralErrorCodes.ReferralCompletionNotAllowed, ex.Message);
        }

        var accessGrant = await referralRepository.GetAccessGrantByReferralIdAsync(referral.Id, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.ReferralAccessGrantNotFound,
                "Referral access grant was not found.");
        accessGrant.Revoke(completedAtUtc);
        await referralRepository.UpdateAccessGrantAsync(accessGrant, ct);

        await referralRepository.UpdateAsync(referral, ct);
        foreach (var domainEvent in referral.DomainEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }
        referral.ClearDomainEvents();

        return referral.ToDto();
    }

    private async Task<Domain.Identity.Doctor> ResolveDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");
    }
}
