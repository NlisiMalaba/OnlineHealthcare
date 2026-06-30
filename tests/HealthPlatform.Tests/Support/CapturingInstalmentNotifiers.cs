using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Domain.Payments.Instalments;
using Xunit;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingInstalmentDueReminderNotifier : IInstalmentDueReminderNotifier
{
    public List<(
        Guid PatientUserId,
        Guid PatientId,
        Guid InstalmentPlanId,
        Guid InstalmentPaymentId,
        int SequenceNumber,
        long AmountMinorUnits,
        string Currency,
        DateOnly DueDate)> Notifications { get; } = [];

    public Task NotifyDueReminderAsync(
        Guid patientUserId,
        Guid patientId,
        Guid instalmentPlanId,
        Guid instalmentPaymentId,
        int sequenceNumber,
        long amountMinorUnits,
        string currency,
        DateOnly dueDate,
        CancellationToken ct)
    {
        Notifications.Add((
            patientUserId,
            patientId,
            instalmentPlanId,
            instalmentPaymentId,
            sequenceNumber,
            amountMinorUnits,
            currency,
            dueDate));
        return Task.CompletedTask;
    }
}

public sealed class CapturingInstalmentMissedPaymentNotifier : IInstalmentMissedPaymentNotifier
{
    public List<(
        Guid PatientUserId,
        Guid PatientId,
        Guid InstalmentPlanId,
        Guid InstalmentPaymentId,
        int SequenceNumber,
        long AmountMinorUnits,
        long LateFeeMinorUnits,
        string Currency,
        DateOnly DueDate)> Notifications { get; } = [];

    public Task NotifyMissedPaymentAsync(
        Guid patientUserId,
        Guid patientId,
        Guid instalmentPlanId,
        Guid instalmentPaymentId,
        int sequenceNumber,
        long amountMinorUnits,
        long lateFeeMinorUnits,
        string currency,
        DateOnly dueDate,
        CancellationToken ct)
    {
        Notifications.Add((
            patientUserId,
            patientId,
            instalmentPlanId,
            instalmentPaymentId,
            sequenceNumber,
            amountMinorUnits,
            lateFeeMinorUnits,
            currency,
            dueDate));
        return Task.CompletedTask;
    }
}
