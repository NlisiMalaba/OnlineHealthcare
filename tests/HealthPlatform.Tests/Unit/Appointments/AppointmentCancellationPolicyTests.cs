using HealthPlatform.Domain.Appointments;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class AppointmentCancellationPolicyTests
{
    [Fact]
    public void Cancel_more_than_two_hours_before_is_early_cancellation()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 25, 14, 0, 0, DateTimeKind.Utc);
        var cancelledAtUtc = scheduledAtUtc.AddHours(-2).AddMinutes(-1);
        var appointment = CreateConfirmedAppointment(scheduledAtUtc);

        var outcome = appointment.Cancel(
            cancelledAtUtc,
            TimeSpan.FromHours(2),
            100m,
            null);

        Assert.True(outcome.IsEarlyCancellation);
        Assert.Equal(0m, outcome.AppliedLateCancellationRetentionPercent);
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public void Cancel_exactly_two_hours_before_applies_late_cancellation_policy()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 25, 14, 0, 0, DateTimeKind.Utc);
        var cancelledAtUtc = scheduledAtUtc.AddHours(-2);
        var appointment = CreateConfirmedAppointment(scheduledAtUtc);

        var outcome = appointment.Cancel(
            cancelledAtUtc,
            TimeSpan.FromHours(2),
            50m,
            null);

        Assert.False(outcome.IsEarlyCancellation);
        Assert.Equal(50m, outcome.AppliedLateCancellationRetentionPercent);
    }

    [Fact]
    public void Cancel_less_than_two_hours_before_applies_doctor_retention_percent()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 25, 14, 0, 0, DateTimeKind.Utc);
        var cancelledAtUtc = scheduledAtUtc.AddHours(-1);
        var appointment = CreateConfirmedAppointment(scheduledAtUtc);

        var outcome = appointment.Cancel(
            cancelledAtUtc,
            TimeSpan.FromHours(2),
            25m,
            null);

        Assert.False(outcome.IsEarlyCancellation);
        Assert.Equal(25m, outcome.AppliedLateCancellationRetentionPercent);
    }

    private static Appointment CreateConfirmedAppointment(DateTime scheduledAtUtc)
    {
        var appointment = Appointment.CreatePendingPayment(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            scheduledAtUtc,
            DateTime.UtcNow.AddMinutes(10));

        appointment.ConfirmOnPayment(scheduledAtUtc.AddDays(-1));
        return appointment;
    }
}
