using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.MentalHealth.MoodLogs.Notifications;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.EventHandlers;

public sealed class ConsecutiveLowMoodDetectedNotificationHandler(
    IPatientRepository patientRepository,
    IConsecutiveLowMoodPromptNotifier consecutiveLowMoodPromptNotifier)
    : INotificationHandler<ConsecutiveLowMoodDetectedNotification>
{
    public async Task Handle(ConsecutiveLowMoodDetectedNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException(
                MoodLogErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        await consecutiveLowMoodPromptNotifier.NotifyPatientAsync(
            patient.UserId,
            notification.PatientId,
            notification.TriggeringMoodLogId,
            ct);
    }
}
