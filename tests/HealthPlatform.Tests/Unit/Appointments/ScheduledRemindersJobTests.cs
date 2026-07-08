using HealthPlatform.Application.Appointments;
using HealthPlatform.Infrastructure.Jobs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class ScheduledRemindersJobTests
{
    [Fact]
    public async Task RunAsync_dispatches_appointment_reminders()
    {
        var appointmentDispatcher = new Mock<IAppointmentReminderDispatcher>();
        appointmentDispatcher
            .Setup(d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        var referralDispatcher = new Mock<HealthPlatform.Application.Referrals.IReferralTimeoutReminderDispatcher>();
        referralDispatcher
            .Setup(d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var job = new ScheduledRemindersJob(
            appointmentDispatcher.Object,
            referralDispatcher.Object,
            NullLogger<ScheduledRemindersJob>.Instance);

        await job.RunAsync(CancellationToken.None);

        appointmentDispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        referralDispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
