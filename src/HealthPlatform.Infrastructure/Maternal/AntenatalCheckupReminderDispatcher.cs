using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Domain.Maternal;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Maternal;

public sealed class AntenatalCheckupReminderDispatcher(
    TimeProvider timeProvider,
    IAntenatalRecordRepository antenatalRecordRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAntenatalCheckupReminderNotifier notifier,
    ILogger<AntenatalCheckupReminderDispatcher> logger) : IAntenatalCheckupReminderDispatcher
{
    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var dueRecords = await antenatalRecordRepository.ListActiveDueForReminderAsync(now, ct);

        if (dueRecords.Count == 0)
        {
            return 0;
        }

        var dispatched = 0;
        foreach (var record in dueRecords)
        {
            ct.ThrowIfCancellationRequested();

            var patient = await patientRepository.GetByIdAsync(record.PatientId, ct);
            if (patient is null)
            {
                logger.LogWarning(
                    "Skipping antenatal reminder for record {AntenatalRecordId}; patient {PatientId} was not found.",
                    record.Id,
                    record.PatientId);
                continue;
            }

            var doctor = await doctorRepository.GetByIdAsync(record.ObstetricDoctorId, ct);
            if (doctor is null)
            {
                logger.LogWarning(
                    "Skipping antenatal reminder for record {AntenatalRecordId}; doctor {DoctorId} was not found.",
                    record.Id,
                    record.ObstetricDoctorId);
                continue;
            }

            var highFrequency = AntenatalReminderPolicies.IsWithinDueDateProximity(
                record.EstimatedDueDate,
                DateOnly.FromDateTime(now));

            await notifier.NotifyAntenatalCheckupReminderAsync(
                patient.UserId,
                doctor.UserId,
                record.Id,
                record.EstimatedDueDate,
                highFrequency,
                ct);

            if (!record.MarkReminderSent(now))
            {
                continue;
            }

            await antenatalRecordRepository.UpdateAsync(record, ct);
            dispatched++;

            logger.LogInformation(
                "Dispatched antenatal checkup reminder for record {AntenatalRecordId}; high frequency {HighFrequency}.",
                record.Id,
                highFrequency);
        }

        return dispatched;
    }
}
