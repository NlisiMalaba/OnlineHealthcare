using HealthPlatform.Application.Maternal.AntenatalRecords;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Maternal;

public sealed class LoggingAntenatalRecordCreatedNotifier(
    ILogger<LoggingAntenatalRecordCreatedNotifier> logger)
    : IAntenatalRecordCreatedNotifier
{
    public Task NotifyAntenatalRecordCreatedAsync(
        Guid patientUserId,
        Guid obstetricDoctorUserId,
        Guid antenatalRecordId,
        DateOnly estimatedDueDate,
        int recommendedCheckupCount,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Antenatal record created notification for record {AntenatalRecordId}, patient user {PatientUserId}, obstetric doctor user {ObstetricDoctorUserId}, due date {EstimatedDueDate}, checkup count {CheckupCount}.",
            antenatalRecordId,
            patientUserId,
            obstetricDoctorUserId,
            estimatedDueDate,
            recommendedCheckupCount);
        return Task.CompletedTask;
    }
}
