using HealthPlatform.Application.Maternal.AntenatalRecords;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Maternal;

public sealed class LoggingAntenatalCheckupReminderNotifier(
    ILogger<LoggingAntenatalCheckupReminderNotifier> logger)
    : IAntenatalCheckupReminderNotifier
{
    public Task NotifyAntenatalCheckupReminderAsync(
        Guid patientUserId,
        Guid obstetricDoctorUserId,
        Guid antenatalRecordId,
        DateOnly estimatedDueDate,
        bool highFrequency,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Antenatal checkup reminder for record {AntenatalRecordId}, patient user {PatientUserId}, obstetric doctor user {ObstetricDoctorUserId}, due date {EstimatedDueDate}, high frequency {HighFrequency}.",
            antenatalRecordId,
            patientUserId,
            obstetricDoctorUserId,
            estimatedDueDate,
            highFrequency);
        return Task.CompletedTask;
    }
}
