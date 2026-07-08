using HealthPlatform.Application.Referrals;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingReferralStatusChangedNotifier : IReferralStatusChangedNotifier
{
    public List<Call> Calls { get; } = [];

    public Task NotifyReferralStatusChangedAsync(
        Guid patientUserId,
        Guid referringDoctorUserId,
        Guid referralId,
        string status,
        string? reason,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new Call(
            patientUserId,
            referringDoctorUserId,
            referralId,
            status,
            reason));
        return Task.CompletedTask;
    }

    public sealed record Call(
        Guid PatientUserId,
        Guid ReferringDoctorUserId,
        Guid ReferralId,
        string Status,
        string? Reason);
}
