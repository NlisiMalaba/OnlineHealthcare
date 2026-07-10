using HealthPlatform.Application.Referrals;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingReferralCreatedNotifier : IReferralCreatedNotifier
{
    public List<Call> Calls { get; } = [];

    public Task NotifyReferralCreatedAsync(
        Guid patientUserId,
        Guid? receivingDoctorUserId,
        Guid referralId,
        Guid referringDoctorId,
        string reason,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new Call(
            patientUserId,
            receivingDoctorUserId,
            referralId,
            referringDoctorId,
            reason));
        return Task.CompletedTask;
    }

    public sealed record Call(
        Guid PatientUserId,
        Guid? ReceivingDoctorUserId,
        Guid ReferralId,
        Guid ReferringDoctorId,
        string Reason);
}
