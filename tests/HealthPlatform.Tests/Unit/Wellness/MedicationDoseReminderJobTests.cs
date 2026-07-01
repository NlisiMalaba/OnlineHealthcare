using HealthPlatform.Application.Wellness;
using HealthPlatform.Infrastructure.Jobs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class MedicationDoseReminderJobTests
{
    [Fact]
    public async Task RunAsync_dispatches_medication_dose_reminders()
    {
        var dispatcher = new Mock<IMedicationDoseReminderDispatcher>();
        dispatcher
            .Setup(d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var job = new MedicationDoseReminderJob(dispatcher.Object, NullLogger<MedicationDoseReminderJob>.Instance);

        await job.RunAsync(CancellationToken.None);

        dispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
