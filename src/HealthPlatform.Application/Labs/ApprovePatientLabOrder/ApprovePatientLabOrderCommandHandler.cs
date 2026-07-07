using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Labs;
using MediatR;

namespace HealthPlatform.Application.Labs.ApprovePatientLabOrder;

public sealed class ApprovePatientLabOrderCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    ILabOrderRepository labOrderRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    ILabPartnerOrderClient labPartnerOrderClient,
    TimeProvider timeProvider) : IRequestHandler<ApprovePatientLabOrderCommand, LabOrderDto>
{
    public async Task<LabOrderDto> Handle(ApprovePatientLabOrderCommand request, CancellationToken ct)
    {
        var doctor = await ResolveVerifiedDoctorAsync(ct);
        var order = await labOrderRepository.GetByIdAsync(request.LabOrderId, ct)
            ?? throw new NotFoundException(LabOrderErrorCodes.LabOrderNotFound, "Lab order was not found.");

        order.Approve(doctor.Id, timeProvider.GetUtcNow().UtcDateTime);
        var reference = await labPartnerOrderClient.SubmitOrderAsync(
            new LabPartnerOrderSubmission(
                order.Id,
                order.PatientId,
                order.LabPartnerCode,
                order.TestCode,
                order.ClinicalNotes),
            ct);
        order.MarkSubmitted(reference);

        await healthRecordEntryRepository.AddAsync(
            new HealthRecordEntryCreateModel(
                order.HealthRecordId,
                HealthRecordEntryType.LabOrderRef,
                new HealthRecordEntryContentPayload(
                    LabOrderRef: new LabOrderRefContent(order.Id, order.TestCode, order.LabPartnerCode)),
                doctor.Id,
                timeProvider.GetUtcNow().UtcDateTime,
                true),
            ct);

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
                "Only verified doctors can approve patient-requested lab orders.");
        }

        return doctor;
    }
}
