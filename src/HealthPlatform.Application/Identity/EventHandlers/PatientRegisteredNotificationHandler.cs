using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Domain.HealthRecords;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Identity.EventHandlers;

public sealed class PatientRegisteredNotificationHandler(
    IHealthRecordRepository healthRecordRepository,
    ILogger<PatientRegisteredNotificationHandler> logger) : INotificationHandler<PatientRegisteredNotification>
{
    public async Task Handle(PatientRegisteredNotification notification, CancellationToken ct)
    {
        if (await healthRecordRepository.ExistsForPatientAsync(notification.PatientId, ct))
        {
            logger.LogDebug(
                "Health record already exists for patient {PatientId}; skipping creation.",
                notification.PatientId);
            return;
        }

        var healthRecord = HealthRecord.CreateForPatient(notification.PatientId);
        await healthRecordRepository.AddAsync(healthRecord, ct);
        await healthRecordRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created health record {HealthRecordId} for patient {PatientId}.",
            healthRecord.Id,
            notification.PatientId);
    }
}
