using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Application.Wellness.CarePlans;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class CarePlanReviewReminderDispatcherTests
{
    [Fact]
    public async Task DispatchDueReminders_notifies_assigned_doctor_when_review_interval_reached()
    {
        var now = new DateTime(2026, 7, 23, 9, 0, 0, DateTimeKind.Utc);
        var asOfDate = DateOnly.FromDateTime(now);
        var doctor = CreateVerifiedDoctor(Guid.CreateVersion7());
        var plan = CarePlan.Assign(
            Guid.CreateVersion7(),
            doctor.Id,
            "Asthma",
            [new CarePlanTaskDraft(Guid.Empty, "Inhaler technique check", null, asOfDate.AddDays(-2))],
            [new CarePlanMonitoringTargetDraft("Peak flow", 400m, "L/min")],
            reviewIntervalDays: 14,
            assignedAtUtc: now.AddDays(-14));

        Assert.Equal(asOfDate, plan.NextReviewAt);
        Assert.True(plan.IsDueForReviewReminder(asOfDate));

        var carePlanRepository = new Mock<ICarePlanRepository>();
        carePlanRepository
            .Setup(repo => repo.ListDueForReviewReminderAsync(
                asOfDate,
                WellnessPolicies.CarePlanReminderBatchSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([plan]);

        CarePlan? updated = null;
        carePlanRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<CarePlan>(), It.IsAny<CancellationToken>()))
            .Callback<CarePlan, CancellationToken>((entity, _) => updated = entity)
            .Returns(Task.CompletedTask);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository
            .Setup(repo => repo.GetByIdAsync(doctor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var notifier = new Mock<ICarePlanReviewReminderNotifier>();
        var dispatcher = new CarePlanReviewReminderDispatcher(
            new FakeTimeProvider(now),
            carePlanRepository.Object,
            doctorRepository.Object,
            notifier.Object,
            NullLogger<CarePlanReviewReminderDispatcher>.Instance);

        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, dispatched);
        notifier.Verify(
            n => n.NotifyReviewDueAsync(
                doctor.UserId,
                plan.Id,
                plan.PatientId,
                plan.Condition,
                plan.NextReviewAt,
                It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.NotNull(updated);
        Assert.Equal(now, updated.ReviewReminderSentAtUtc);
        Assert.False(updated.IsDueForReviewReminder(asOfDate));
    }

    [Fact]
    public async Task DispatchDueReminders_is_idempotent_after_review_reminder_sent()
    {
        var now = new DateTime(2026, 7, 23, 9, 0, 0, DateTimeKind.Utc);
        var asOfDate = DateOnly.FromDateTime(now);
        var doctor = CreateVerifiedDoctor(Guid.CreateVersion7());
        var plan = CarePlan.Assign(
            Guid.CreateVersion7(),
            doctor.Id,
            "COPD",
            [new CarePlanTaskDraft(Guid.Empty, "Spirometry prep", null, asOfDate.AddDays(-1))],
            [new CarePlanMonitoringTargetDraft("FEV1", 70m, "%")],
            reviewIntervalDays: 7,
            assignedAtUtc: now.AddDays(-7));

        var carePlanRepository = new Mock<ICarePlanRepository>();
        carePlanRepository
            .SetupSequence(repo => repo.ListDueForReviewReminderAsync(
                asOfDate,
                WellnessPolicies.CarePlanReminderBatchSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([plan])
            .ReturnsAsync([]);

        carePlanRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<CarePlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository
            .Setup(repo => repo.GetByIdAsync(doctor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var notifier = new Mock<ICarePlanReviewReminderNotifier>();
        var dispatcher = new CarePlanReviewReminderDispatcher(
            new FakeTimeProvider(now),
            carePlanRepository.Object,
            doctorRepository.Object,
            notifier.Object,
            NullLogger<CarePlanReviewReminderDispatcher>.Instance);

        var firstRun = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);
        var secondRun = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, firstRun);
        Assert.Equal(0, secondRun);
        notifier.Verify(
            n => n.NotifyReviewDueAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchDueReminders_skips_when_doctor_missing()
    {
        var now = new DateTime(2026, 7, 23, 9, 0, 0, DateTimeKind.Utc);
        var asOfDate = DateOnly.FromDateTime(now);
        var doctorId = Guid.CreateVersion7();
        var plan = CarePlan.Assign(
            Guid.CreateVersion7(),
            doctorId,
            "Heart failure",
            [new CarePlanTaskDraft(Guid.Empty, "Daily weight", null, asOfDate)],
            [new CarePlanMonitoringTargetDraft("Weight", 80m, "kg")],
            reviewIntervalDays: 10,
            assignedAtUtc: now.AddDays(-10));

        var carePlanRepository = new Mock<ICarePlanRepository>();
        carePlanRepository
            .Setup(repo => repo.ListDueForReviewReminderAsync(
                asOfDate,
                WellnessPolicies.CarePlanReminderBatchSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([plan]);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository
            .Setup(repo => repo.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        var notifier = new Mock<ICarePlanReviewReminderNotifier>();
        var dispatcher = new CarePlanReviewReminderDispatcher(
            new FakeTimeProvider(now),
            carePlanRepository.Object,
            doctorRepository.Object,
            notifier.Object,
            NullLogger<CarePlanReviewReminderDispatcher>.Instance);

        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(0, dispatched);
        notifier.Verify(
            n => n.NotifyReviewDueAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        carePlanRepository.Verify(
            repo => repo.UpdateAsync(It.IsAny<CarePlan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Doctor CreateVerifiedDoctor(Guid userId)
    {
        var doctorId = Guid.CreateVersion7();
        var doctor = Doctor.Register(
            doctorId,
            userId,
            "Dr. Care Plan Review",
            "LIC-CARE-01",
            "Internal Medicine",
            10,
            "Harare Clinic",
            new GeoPoint(-17.8, 31.0),
            40m,
            60m,
            null,
            "care-plan-doctor@example.com",
            "+263771111111",
            null,
            null,
            [
                DoctorAvailabilitySlot.Create(
                    doctorId,
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(12, 0),
                    30,
                    DoctorAppointmentType.Both)
            ]);
        doctor.VerifyLicense();
        return doctor;
    }
}
