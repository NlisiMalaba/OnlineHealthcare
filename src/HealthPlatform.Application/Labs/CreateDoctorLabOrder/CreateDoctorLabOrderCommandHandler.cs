using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Labs;
using MediatR;

namespace HealthPlatform.Application.Labs.CreateDoctorLabOrder;

public sealed class CreateDoctorLabOrderCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IPatientRepository patientRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    ILabOrderRepository labOrderRepository,
    ILabPartnerOrderClient labPartnerOrderClient,
    TimeProvider timeProvider) : IRequestHandler<CreateDoctorLabOrderCommand, LabOrderDto>
{
    public async Task<LabOrderDto> Handle(CreateDoctorLabOrderCommand request, CancellationToken ct)
    {
        var doctor = await ResolveVerifiedDoctorAsync(ct);
        var patient = await patientRepository.GetByIdAsync(request.PatientId, ct)
            ?? throw new NotFoundException(LabOrderErrorCodes.PatientNotFound, "Patient was not found.");
        var healthRecord = await healthRecordRepository.GetByIdAsync(request.HealthRecordId, ct)
            ?? throw new NotFoundException(LabOrderErrorCodes.HealthRecordNotFound, "Health record was not found.");

        EnsureHealthRecordOwnership(healthRecord.PatientId, patient.Id);

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var order = LabOrder.CreateDoctorOrdered(
            patient.Id,
            healthRecord.Id,
            doctor.Id,
            request.LabPartnerCode,
            request.TestCode,
            request.ClinicalNotes,
            nowUtc);

        var reference = await labPartnerOrderClient.SubmitOrderAsync(
            new LabPartnerOrderSubmission(
                order.Id,
                order.PatientId,
                order.LabPartnerCode,
                order.TestCode,
                order.ClinicalNotes),
            ct);
        order.MarkSubmitted(reference);

        await labOrderRepository.AddAsync(order, ct);
        await AttachOrderToHealthRecordAsync(order, ct);
        await labOrderRepository.SaveChangesAsync(ct);
        return order.ToDto();
    }

    private async Task<Doctor> ResolveVerifiedDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated doctor is required.");
        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(LabOrderErrorCodes.DoctorNotFound, "Doctor profile was not found.");
        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            throw new DomainException(
                LabOrderErrorCodes.DoctorNotVerified,
                "Only verified doctors can create doctor-ordered lab requests.");
        }

        return doctor;
    }

    private static void EnsureHealthRecordOwnership(Guid healthRecordPatientId, Guid expectedPatientId)
    {
        if (healthRecordPatientId != expectedPatientId)
        {
            throw new DomainException(
                LabOrderErrorCodes.HealthRecordOwnershipMismatch,
                "Health record does not belong to the selected patient.");
        }
    }

    private async Task AttachOrderToHealthRecordAsync(LabOrder order, CancellationToken ct)
    {
        await healthRecordEntryRepository.AddAsync(
            new HealthRecordEntryCreateModel(
                order.HealthRecordId,
                HealthRecordEntryType.LabOrderRef,
                new HealthRecordEntryContentPayload(
                    LabOrderRef: new LabOrderRefContent(order.Id, order.TestCode, order.LabPartnerCode)),
                order.OrderingDoctorId ?? Guid.Empty,
                timeProvider.GetUtcNow().UtcDateTime,
                true),
            ct);
    }
}
