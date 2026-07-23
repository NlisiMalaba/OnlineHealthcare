using HealthPlatform.Domain.Wellness;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class CarePlanProgressCalculatorTests
{
    [Fact]
    public void Calculate_empty_tasks_returns_zero_progress()
    {
        var progress = CarePlanProgressCalculator.Calculate([]);

        Assert.Equal(0, progress.CompletedTaskCount);
        Assert.Equal(0, progress.TotalTaskCount);
        Assert.Equal(0m, progress.PercentComplete);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 50)]
    [InlineData(2, 100)]
    public void Calculate_reports_percent_from_completed_tasks(int completedCount, decimal expectedPercent)
    {
        var now = new DateTime(2026, 7, 23, 10, 0, 0, DateTimeKind.Utc);
        var tasks = new List<CarePlanTask>
        {
            new(Guid.CreateVersion7(), "Check glucose", null, new DateOnly(2026, 7, 20), null, null),
            new(Guid.CreateVersion7(), "Walk 30 minutes", null, new DateOnly(2026, 7, 21), null, null)
        };

        for (var i = 0; i < completedCount; i++)
        {
            tasks[i] = tasks[i].MarkCompleted(now);
        }

        var progress = CarePlanProgressCalculator.Calculate(tasks);

        Assert.Equal(completedCount, progress.CompletedTaskCount);
        Assert.Equal(2, progress.TotalTaskCount);
        Assert.Equal(expectedPercent, progress.PercentComplete);
    }

    [Fact]
    public void CarePlan_progress_updates_after_task_completion()
    {
        var assignedAt = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
        var plan = CarePlan.Assign(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Hypertension",
            [
                new CarePlanTaskDraft(Guid.Empty, "Measure BP", null, new DateOnly(2026, 7, 5)),
                new CarePlanTaskDraft(Guid.Empty, "Take medication", null, new DateOnly(2026, 7, 6)),
                new CarePlanTaskDraft(Guid.Empty, "Low-salt meal plan", null, new DateOnly(2026, 7, 7))
            ],
            [new CarePlanMonitoringTargetDraft("Systolic BP", 130m, "mmHg")],
            30,
            assignedAt);

        Assert.Equal(0, plan.Progress.CompletedTaskCount);
        Assert.Equal(0m, plan.Progress.PercentComplete);

        var completedAt = new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc);
        plan.CompleteTask(plan.Tasks[0].Id, completedAt);

        Assert.Equal(1, plan.Progress.CompletedTaskCount);
        Assert.Equal(3, plan.Progress.TotalTaskCount);
        Assert.Equal(33.33m, plan.Progress.PercentComplete);
        Assert.Equal(completedAt, plan.Tasks[0].CompletedAtUtc);
    }
}
