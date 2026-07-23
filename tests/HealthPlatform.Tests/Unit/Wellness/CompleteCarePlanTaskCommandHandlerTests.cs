using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Application.Wellness.CarePlans;
using HealthPlatform.Application.Wellness.CarePlans.CompleteCarePlanTask;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class CompleteCarePlanTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_records_completion_timestamp_and_updates_progress()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Care Plan Patient", "care-plan@example.com");
        var completedAt = new DateTime(2026, 7, 23, 11, 30, 0, DateTimeKind.Utc);
        var plan = CreateActivePlan(patient.Id, Guid.CreateVersion7(), completedAt.AddDays(-10));
        var taskId = plan.Tasks[0].Id;

        var currentUser = new TestCurrentUserAccessor { UserId = patientUserId };
        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(repo => repo.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var carePlanRepository = new Mock<ICarePlanRepository>();
        carePlanRepository
            .Setup(repo => repo.GetByIdForPatientAsync(plan.Id, patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        CarePlan? updated = null;
        carePlanRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<CarePlan>(), It.IsAny<CancellationToken>()))
            .Callback<CarePlan, CancellationToken>((entity, _) => updated = entity)
            .Returns(Task.CompletedTask);

        var handler = new CompleteCarePlanTaskCommandHandler(
            currentUser,
            patientRepository.Object,
            carePlanRepository.Object,
            new FakeTimeProvider(completedAt));

        var result = await handler.Handle(
            new CompleteCarePlanTaskCommand(plan.Id, taskId),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(completedAt, result.Tasks.Single(task => task.Id == taskId).CompletedAtUtc);
        Assert.True(result.Tasks.Single(task => task.Id == taskId).IsCompleted);
        Assert.Equal(1, result.Progress.CompletedTaskCount);
        Assert.Equal(50m, result.Progress.PercentComplete);
        carePlanRepository.Verify(
            repo => repo.UpdateAsync(plan, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_rejects_second_completion_of_same_task()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Care Plan Patient", "care-plan-dup@example.com");
        var firstCompletedAt = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc);
        var plan = CreateActivePlan(patient.Id, Guid.CreateVersion7(), firstCompletedAt.AddDays(-5));
        var taskId = plan.Tasks[0].Id;
        plan.CompleteTask(taskId, firstCompletedAt);

        var currentUser = new TestCurrentUserAccessor { UserId = patientUserId };
        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(repo => repo.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var carePlanRepository = new Mock<ICarePlanRepository>();
        carePlanRepository
            .Setup(repo => repo.GetByIdForPatientAsync(plan.Id, patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        var handler = new CompleteCarePlanTaskCommandHandler(
            currentUser,
            patientRepository.Object,
            carePlanRepository.Object,
            new FakeTimeProvider(firstCompletedAt.AddHours(2)));

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CompleteCarePlanTaskCommand(plan.Id, taskId), CancellationToken.None));

        Assert.Equal(WellnessErrorCodes.CarePlanTaskAlreadyCompleted, ex.Code);
        carePlanRepository.Verify(
            repo => repo.UpdateAsync(It.IsAny<CarePlan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_rejects_unknown_task()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Care Plan Patient", "care-plan-missing@example.com");
        var now = new DateTime(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);
        var plan = CreateActivePlan(patient.Id, Guid.CreateVersion7(), now.AddDays(-3));

        var currentUser = new TestCurrentUserAccessor { UserId = patientUserId };
        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(repo => repo.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var carePlanRepository = new Mock<ICarePlanRepository>();
        carePlanRepository
            .Setup(repo => repo.GetByIdForPatientAsync(plan.Id, patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        var handler = new CompleteCarePlanTaskCommandHandler(
            currentUser,
            patientRepository.Object,
            carePlanRepository.Object,
            new FakeTimeProvider(now));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CompleteCarePlanTaskCommand(plan.Id, Guid.CreateVersion7()),
                CancellationToken.None));

        Assert.Equal(WellnessErrorCodes.CarePlanTaskNotFound, ex.Code);
    }

    private static CarePlan CreateActivePlan(Guid patientId, Guid doctorId, DateTime assignedAtUtc) =>
        CarePlan.Assign(
            patientId,
            doctorId,
            "Type 2 Diabetes",
            [
                new CarePlanTaskDraft(Guid.Empty, "Log fasting glucose", null, DateOnly.FromDateTime(assignedAtUtc).AddDays(1)),
                new CarePlanTaskDraft(Guid.Empty, "Foot inspection", null, DateOnly.FromDateTime(assignedAtUtc).AddDays(2))
            ],
            [new CarePlanMonitoringTargetDraft("HbA1c", 7m, "%")],
            30,
            assignedAtUtc);
}
