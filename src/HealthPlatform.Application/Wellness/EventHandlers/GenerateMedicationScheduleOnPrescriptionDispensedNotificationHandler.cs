using HealthPlatform.Application.Prescriptions.Notifications;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Wellness;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Wellness.EventHandlers;

public sealed class GenerateMedicationScheduleOnPrescriptionDispensedNotificationHandler(
    IMedicationScheduleRepository medicationScheduleRepository,
    ILogger<GenerateMedicationScheduleOnPrescriptionDispensedNotificationHandler> logger)
    : INotificationHandler<PrescriptionDispensedNotification>
{
    public async Task Handle(PrescriptionDispensedNotification notification, CancellationToken ct)
    {
        var existingSchedule = await medicationScheduleRepository.GetByPrescriptionIdAsync(
            notification.PrescriptionId,
            ct);
        if (existingSchedule is not null)
        {
            logger.LogInformation(
                "Medication schedule already exists for prescription {PrescriptionId}.",
                notification.PrescriptionId);
            return;
        }

        var doseTimes = MedicationDoseSchedulePolicies.BuildDoseTimes(
            notification.Frequency,
            notification.DurationDays,
            notification.DispensedAtUtc);

        var schedule = MedicationSchedule.CreateActive(
            notification.PrescriptionId,
            notification.PatientId,
            notification.MedicationName,
            doseTimes);

        await medicationScheduleRepository.AddAsync(schedule, ct);

        logger.LogInformation(
            "Generated medication schedule {ScheduleId} with {DoseCount} doses for prescription {PrescriptionId}.",
            schedule.Id,
            schedule.DoseTimes.Count,
            notification.PrescriptionId);
    }
}
