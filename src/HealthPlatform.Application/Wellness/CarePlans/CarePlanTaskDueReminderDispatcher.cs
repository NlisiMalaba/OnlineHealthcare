using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Wellness;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Wellness.CarePlans;

public sealed class CarePlanTaskDueReminderDispatcher(
    TimeProvider timeProvider,
    ICarePlanRepository carePlanRepository,
    IPatientRepository patientRepository,
    ICarePlanTaskDueReminderNotifier notifier,
    ILogger<CarePlanTaskDueReminderDispatcher> logger) : ICarePlanTaskDueReminderDispatcher
{
    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var asOfDate = DateOnly.FromDateTime(nowUtc);
        var plans = await carePlanRepository.ListActiveForTaskRemindersAsync(
            WellnessPolicies.CarePlanReminderBatchSize,
            ct);

        if (plans.Count == 0)
        {
            return 0;
        }

        var dispatched = 0;
        foreach (var plan in plans)
        {
            ct.ThrowIfCancellationRequested();

            var dueTasks = plan.ListDueTasksForReminder(asOfDate);
            if (dueTasks.Count == 0)
            {
                continue;
            }

            var patient = await patientRepository.GetByIdAsync(plan.PatientId, ct);
            if (patient is null)
            {
                logger.LogWarning(
                    "Skipping care plan task reminders for plan {CarePlanId}; patient {PatientId} was not found.",
                    plan.Id,
                    plan.PatientId);
                continue;
            }

            var planDirty = false;
            foreach (var task in dueTasks)
            {
                ct.ThrowIfCancellationRequested();

                await notifier.NotifyTaskDueAsync(
                    patient.UserId,
                    plan.Id,
                    task.Id,
                    plan.Condition,
                    task.Title,
                    task.DueDate,
                    ct);

                if (!plan.MarkTaskReminderSent(task.Id, nowUtc))
                {
                    continue;
                }

                planDirty = true;
                dispatched++;

                logger.LogInformation(
                    "Dispatched care plan task reminder for plan {CarePlanId}, task {TaskId}.",
                    plan.Id,
                    task.Id);
            }

            if (planDirty)
            {
                await carePlanRepository.UpdateAsync(plan, ct);
            }
        }

        return dispatched;
    }
}
