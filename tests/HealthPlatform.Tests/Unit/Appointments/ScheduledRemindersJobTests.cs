using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Application.Wellness.CarePlans;
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
        var antenatalDispatcher = new Mock<IAntenatalCheckupReminderDispatcher>();
        antenatalDispatcher
            .Setup(d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        var fetalMonitoringDispatcher = new Mock<IFetalMonitoringReminderDispatcher>();
        fetalMonitoringDispatcher
            .Setup(d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var vaccinationDispatcher = new Mock<IVaccinationReminderDispatcher>();
        vaccinationDispatcher
            .Setup(d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        var carePlanTaskDispatcher = new Mock<ICarePlanTaskDueReminderDispatcher>();
        carePlanTaskDispatcher
            .Setup(d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        var carePlanReviewDispatcher = new Mock<ICarePlanReviewReminderDispatcher>();
        carePlanReviewDispatcher
            .Setup(d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var job = new ScheduledRemindersJob(
            appointmentDispatcher.Object,
            referralDispatcher.Object,
            antenatalDispatcher.Object,
            fetalMonitoringDispatcher.Object,
            vaccinationDispatcher.Object,
            carePlanTaskDispatcher.Object,
            carePlanReviewDispatcher.Object,
            NullLogger<ScheduledRemindersJob>.Instance);

        await job.RunAsync(CancellationToken.None);

        appointmentDispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        referralDispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        antenatalDispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        fetalMonitoringDispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        vaccinationDispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        carePlanTaskDispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        carePlanReviewDispatcher.Verify(
            d => d.DispatchDueRemindersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
