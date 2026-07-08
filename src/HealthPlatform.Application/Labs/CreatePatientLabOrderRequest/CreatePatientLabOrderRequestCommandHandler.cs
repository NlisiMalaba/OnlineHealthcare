using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Labs;
using MediatR;

namespace HealthPlatform.Application.Labs.CreatePatientLabOrderRequest;

public sealed class CreatePatientLabOrderRequestCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthRecordRepository healthRecordRepository,
    ILabOrderRepository labOrderRepository,
    TimeProvider timeProvider) : IRequestHandler<CreatePatientLabOrderRequestCommand, LabOrderDto>
{
    public async Task<LabOrderDto> Handle(CreatePatientLabOrderRequestCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");
        var patient = await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(LabOrderErrorCodes.PatientNotFound, "Patient profile was not found.");
        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(patient.Id, ct)
            ?? throw new NotFoundException(LabOrderErrorCodes.HealthRecordNotFound, "Health record was not found.");

        var order = LabOrder.CreatePatientRequested(
            patient.Id,
            healthRecord.Id,
            request.LabPartnerCode,
            request.TestCode,
            request.ClinicalNotes,
            timeProvider.GetUtcNow().UtcDateTime);

        await labOrderRepository.AddAsync(order, ct);
        await labOrderRepository.SaveChangesAsync(ct);
        return order.ToDto();
    }
}
