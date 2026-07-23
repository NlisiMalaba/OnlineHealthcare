namespace HealthPlatform.Domain.Wellness;

public sealed record CarePlanProgress(
    int CompletedTaskCount,
    int TotalTaskCount,
    decimal PercentComplete);

public static class CarePlanProgressCalculator
{
    public static CarePlanProgress Calculate(IReadOnlyList<CarePlanTask> tasks)
    {
        var total = tasks.Count;
        if (total == 0)
        {
            return new CarePlanProgress(0, 0, 0m);
        }

        var completed = tasks.Count(task => task.IsCompleted);
        var percent = Math.Round(completed * 100m / total, 2, MidpointRounding.AwayFromZero);
        return new CarePlanProgress(completed, total, percent);
    }
}
