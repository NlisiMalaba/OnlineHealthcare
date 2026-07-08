using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Referrals;
using MediatR;

namespace HealthPlatform.Application.Referrals.CreateReferral;

public sealed class CreateReferralCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IPatientRepository patientRepository,
    IReferralRepository referralRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<CreateReferralCommand, ReferralDto>
{
    public async Task<ReferralDto> Handle(CreateReferralCommand request, CancellationToken ct)
    {
        var referringDoctor = await ResolveVerifiedDoctorAsync(ct);

        _ = await patientRepository.GetByIdAsync(request.PatientId, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        if (request.ReceivingDoctorId.HasValue)
        {
            _ = await doctorRepository.GetByIdAsync(request.ReceivingDoctorId.Value, ct)
                ?? throw new NotFoundException(
                    ReferralErrorCodes.ReceivingDoctorNotFound,
                    "Receiving doctor profile was not found.");
        }

        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var referral = Referral.Create(
            request.PatientId,
            referringDoctor.Id,
            request.ReceivingDoctorId,
            request.ReceivingHospitalName,
            request.Reason,
            request.ClinicalNotes,
            request.SharedHealthRecordSections,
            request.PatientConsentAtUtc,
            createdAtUtc);

        await referralRepository.AddAsync(referral, ct);
        foreach (var domainEvent in referral.DomainEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }
        referral.ClearDomainEvents();

        return referral.ToDto();
    }

    private async Task<Doctor> ResolveVerifiedDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            throw new DomainException(
                ReferralErrorCodes.DoctorNotVerified,
                "Only verified doctors can create referrals.");
        }

        return doctor;
    }
}
