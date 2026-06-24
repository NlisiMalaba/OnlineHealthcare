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
        var dispatcher = new Mock<IAppointmentReminderDispatcher>();
        dispatcher
            .Setup(d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var job = new ScheduledRemindersJob(dispatcher.Object, NullLogger<ScheduledRemindersJob>.Instance);

        await job.RunAsync(CancellationToken.None);

        dispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
