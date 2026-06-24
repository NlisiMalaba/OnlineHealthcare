using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Prescriptions;
using MediatR;

namespace HealthPlatform.Application.Prescriptions.CancelPrescription;

public sealed class CancelPrescriptionCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IPrescriptionRepository prescriptionRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<CancelPrescriptionCommand, PrescriptionDto>
{
    public async Task<PrescriptionDto> Handle(CancelPrescriptionCommand request, CancellationToken ct)
    {
        var doctor = await ResolveVerifiedDoctorAsync(ct);

        var prescription = await prescriptionRepository.GetByIdForDoctorAsync(request.PrescriptionId, doctor.Id, ct)
            ?? throw new NotFoundException(
                PrescriptionErrorCodes.PrescriptionNotFound,
                "Prescription was not found.");

        try
        {
            prescription.Cancel(request.Reason, timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (PrescriptionNotCancellableException)
        {
            throw new DomainException(
                PrescriptionErrorCodes.PrescriptionNotCancellable,
                "Only active, non-dispensed prescriptions can be cancelled.");
        }

        await prescriptionRepository.UpdateAsync(prescription, ct);
        await PublishPendingEventsAsync(prescription, ct);

        return prescription.ToDto();
    }

    private async Task<Doctor> ResolveVerifiedDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                PrescriptionErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            throw new DomainException(
                PrescriptionErrorCodes.DoctorNotVerified,
                "Only verified doctors can cancel prescriptions.");
        }

        return doctor;
    }

    private async Task PublishPendingEventsAsync(Prescription prescription, CancellationToken ct)
    {
        var pendingEvents = prescription.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        prescription.ClearDomainEvents();
    }
}
