using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.BirthPlans.Notifications;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.EventHandlers;

public sealed class BirthPlanUpdatedNotificationHandler(
    IDoctorRepository doctorRepository,
    IBirthPlanUpdatedNotifier notifier)
    : INotificationHandler<BirthPlanUpdatedNotification>
{
    public async Task Handle(BirthPlanUpdatedNotification notification, CancellationToken ct)
    {
        var obstetricDoctor = await doctorRepository.GetByIdAsync(notification.ObstetricDoctorId, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.DoctorNotFound,
                "Obstetric doctor profile was not found.");

        await notifier.NotifyBirthPlanUpdatedAsync(
            obstetricDoctor.UserId,
            notification.BirthPlanId,
            notification.AntenatalRecordId,
            notification.PatientId,
            ct);
    }
}
