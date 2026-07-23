namespace HealthPlatform.Application.Wellness.CarePlans;

public interface ICarePlanTaskDueReminderDispatcher
{
    Task<int> DispatchDueRemindersAsync(CancellationToken ct);
}

public interface ICarePlanReviewReminderDispatcher
{
    Task<int> DispatchDueRemindersAsync(CancellationToken ct);
}
