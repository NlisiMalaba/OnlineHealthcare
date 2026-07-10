using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Referrals.Notifications;
using MediatR;

namespace HealthPlatform.Application.Referrals.EventHandlers;

public sealed class ReferralStatusChangedNotificationHandler(
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IReferralStatusChangedNotifier notifier)
    : INotificationHandler<ReferralStatusChangedNotification>
{
    public async Task Handle(ReferralStatusChangedNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        var referringDoctor = await doctorRepository.GetByIdAsync(notification.ReferringDoctorId, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.DoctorNotFound,
                "Referring doctor profile was not found.");

        await notifier.NotifyReferralStatusChangedAsync(
            patient.UserId,
            referringDoctor.UserId,
            notification.ReferralId,
            notification.Status,
            notification.Reason,
            ct);
    }
}
