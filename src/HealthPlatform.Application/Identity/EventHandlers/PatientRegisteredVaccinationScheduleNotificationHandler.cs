using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Application.Vaccinations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Identity.EventHandlers;

public sealed class PatientRegisteredVaccinationScheduleNotificationHandler(
    IPatientRepository patientRepository,
    IVaccinationScheduleInitializer scheduleInitializer,
    TimeProvider timeProvider,
    ILogger<PatientRegisteredVaccinationScheduleNotificationHandler> logger)
    : INotificationHandler<PatientRegisteredNotification>
{
    public async Task Handle(PatientRegisteredNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct);
        if (patient is null)
        {
            logger.LogWarning(
                "Skipping vaccination schedule initialization; patient {PatientId} was not found.",
                notification.PatientId);
            return;
        }

        await scheduleInitializer.InitializePatientScheduleAsync(
            patient,
            timeProvider.GetUtcNow().UtcDateTime,
            ct);

        logger.LogInformation(
            "Initialized vaccination schedule for patient {PatientId}.",
            notification.PatientId);
    }
}
