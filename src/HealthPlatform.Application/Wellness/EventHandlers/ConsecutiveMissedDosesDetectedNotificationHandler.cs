using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Wellness.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Wellness.EventHandlers;

public sealed class ConsecutiveMissedDosesDetectedNotificationHandler(
    INextOfKinRepository nextOfKinRepository,
    IConsecutiveMissedDosesNextOfKinNotifier notifier,
    ILogger<ConsecutiveMissedDosesDetectedNotificationHandler> logger)
    : INotificationHandler<ConsecutiveMissedDosesDetectedNotification>
{
    public async Task Handle(ConsecutiveMissedDosesDetectedNotification notification, CancellationToken ct)
    {
        var contacts = await nextOfKinRepository.ListByPatientIdAsync(notification.PatientId, ct);
        if (contacts.Count == 0)
        {
            logger.LogInformation(
                "Consecutive missed dose alert for patient {PatientId} has no next-of-kin contacts to notify.",
                notification.PatientId);
            return;
        }

        var contactDtos = contacts.Select(contact => contact.ToDto()).ToList();
        await notifier.NotifyConsecutiveMissedDosesAsync(
            notification.PatientId,
            notification.TriggeringAdherenceEventId,
            contactDtos,
            ct);

        logger.LogInformation(
            "Consecutive missed dose alert dispatched to {ContactCount} next-of-kin contact(s) for patient {PatientId}.",
            contactDtos.Count,
            notification.PatientId);
    }
}
