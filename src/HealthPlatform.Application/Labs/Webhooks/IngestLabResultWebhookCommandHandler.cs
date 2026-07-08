using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Labs;
using MediatR;

namespace HealthPlatform.Application.Labs.Webhooks;

public sealed class IngestLabResultWebhookCommandHandler(
    ILabOrderRepository labOrderRepository,
    ILabResultRepository labResultRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IStorageService storageService,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    INotificationDispatcher notificationDispatcher,
    TimeProvider timeProvider) : IRequestHandler<IngestLabResultWebhookCommand, IngestLabResultWebhookResultDto>
{
    public async Task<IngestLabResultWebhookResultDto> Handle(IngestLabResultWebhookCommand request, CancellationToken ct)
    {
        var order = await labOrderRepository.GetByPartnerReferenceAsync(
            request.LabPartnerCode,
            request.LabPartnerOrderReference,
            ct);
        if (order is null)
        {
            throw new NotFoundException(
                LabOrderErrorCodes.LabOrderReferenceNotFound,
                "Lab order matching the provided partner reference was not found.");
        }

        await using var stream = new MemoryStream(request.FileContent, writable: false);
        var upload = await storageService.UploadLabResultAsync(
            order.PatientId,
            order.Id,
            stream,
            request.ContentType,
            request.FileName,
            ct);

        var result = LabResult.Create(
            order.Id,
            order.PatientId,
            order.HealthRecordId,
            order.OrderingDoctorId,
            order.LabPartnerCode,
            order.LabPartnerOrderReference ?? request.LabPartnerOrderReference,
            request.TestCode,
            upload.StorageKey,
            upload.ContentType,
            request.FileName,
            request.IsCritical);

        await labResultRepository.AddAsync(result, ct);
        await healthRecordEntryRepository.AddAsync(
            new HealthRecordEntryCreateModel(
                order.HealthRecordId,
                HealthRecordEntryType.LabResultRef,
                new HealthRecordEntryContentPayload(
                    LabResultRef: new LabResultRefContent(result.Id)),
                order.OrderingDoctorId ?? Guid.Empty,
                timeProvider.GetUtcNow().UtcDateTime,
                true),
            ct);

        var patient = await patientRepository.GetByIdAsync(order.PatientId, ct);
        if (patient is not null)
        {
            await NotifyPatientAsync(patient.UserId, order.Id, result.Id, ct);
        }

        if (order.OrderingDoctorId.HasValue)
        {
            var doctor = await doctorRepository.GetByIdAsync(order.OrderingDoctorId.Value, ct);
            if (doctor is not null)
            {
                await NotifyDoctorAsync(
                    doctor.UserId,
                    order.Id,
                    result.Id,
                    request.IsCritical,
                    ct);
            }
        }

        await labResultRepository.SaveChangesAsync(ct);
        return new IngestLabResultWebhookResultDto(true, result.Id, order.Id, upload.StorageKey);
    }

    private Task NotifyPatientAsync(Guid patientUserId, Guid labOrderId, Guid labResultId, CancellationToken ct) =>
        notificationDispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                patientUserId,
                NotificationRecipientType.Patient,
                NotificationEventTypes.LabResultUploaded,
                NotificationCriticality.Standard,
                new NotificationContent(
                    "Lab result available",
                    "A new lab result has been added to your health record."),
                Metadata: new Dictionary<string, string>
                {
                    ["lab_order_id"] = labOrderId.ToString(),
                    ["lab_result_id"] = labResultId.ToString()
                }),
            ct);

    private Task NotifyDoctorAsync(
        Guid doctorUserId,
        Guid labOrderId,
        Guid labResultId,
        bool isCritical,
        CancellationToken ct) =>
        notificationDispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                doctorUserId,
                NotificationRecipientType.Doctor,
                isCritical ? NotificationEventTypes.CriticalLabResultAlert : NotificationEventTypes.LabResultUploaded,
                isCritical ? NotificationCriticality.Critical : NotificationCriticality.Standard,
                new NotificationContent(
                    isCritical ? "Critical lab result alert" : "Lab result uploaded",
                    isCritical
                        ? "A critical lab result you ordered needs immediate attention."
                        : "A lab result you ordered is now available."),
                Metadata: new Dictionary<string, string>
                {
                    ["lab_order_id"] = labOrderId.ToString(),
                    ["lab_result_id"] = labResultId.ToString()
                }),
            ct);
}
