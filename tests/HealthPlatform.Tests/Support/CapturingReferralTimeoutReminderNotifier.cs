using HealthPlatform.Application.Referrals;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingReferralTimeoutReminderNotifier : IReferralTimeoutReminderNotifier
{
    public List<Call> Calls { get; } = [];

    public Task NotifyReferralTimeoutReminderAsync(
        Guid receivingDoctorUserId,
        Guid referralId,
        Guid patientId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new Call(receivingDoctorUserId, referralId, patientId));
        return Task.CompletedTask;
    }

    public sealed record Call(Guid ReceivingDoctorUserId, Guid ReferralId, Guid PatientId);
}
