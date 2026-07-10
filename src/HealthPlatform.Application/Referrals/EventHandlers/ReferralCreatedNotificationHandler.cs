using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Referrals.Notifications;
using MediatR;

namespace HealthPlatform.Application.Referrals.EventHandlers;

public sealed class ReferralCreatedNotificationHandler(
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IReferralCreatedNotifier notifier)
    : INotificationHandler<ReferralCreatedNotification>
{
    public async Task Handle(ReferralCreatedNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        Guid? receivingDoctorUserId = null;
        if (notification.ReceivingDoctorId.HasValue)
        {
            var receivingDoctor = await doctorRepository.GetByIdAsync(notification.ReceivingDoctorId.Value, ct)
                ?? throw new NotFoundException(
                    ReferralErrorCodes.ReceivingDoctorNotFound,
                    "Receiving doctor profile was not found.");

            receivingDoctorUserId = receivingDoctor.UserId;
        }

        await notifier.NotifyReferralCreatedAsync(
            patient.UserId,
            receivingDoctorUserId,
            notification.ReferralId,
            notification.ReferringDoctorId,
            notification.Reason,
            ct);
    }
}
