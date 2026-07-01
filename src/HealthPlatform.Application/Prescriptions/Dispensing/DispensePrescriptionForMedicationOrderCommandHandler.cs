using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Prescriptions.Dispensing;

public sealed class DispensePrescriptionForMedicationOrderCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IPrescriptionDispensingGuard dispensingGuard,
    IPrescriptionDomainEventPublisher prescriptionDomainEventPublisher)
    : IRequestHandler<DispensePrescriptionForMedicationOrderCommand, PrescriptionDto>
{
    public async Task<PrescriptionDto> Handle(
        DispensePrescriptionForMedicationOrderCommand request,
        CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var patient = await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                PrescriptionErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        var prescription = await dispensingGuard.PrepareDispenseForMedicationOrderAsync(
            request.PrescriptionId,
            patient.Id,
            ct);

        await dispensingGuard.PersistDispensedPrescriptionAsync(prescription, ct);
        await prescriptionDomainEventPublisher.PublishPendingAsync(prescription, ct);

        return prescription.ToDto();
    }
}
