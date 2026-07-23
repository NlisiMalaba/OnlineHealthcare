using HealthPlatform.Application.Identity;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Wellness.CarePlans;

public sealed class CarePlanReviewReminderDispatcher(
    TimeProvider timeProvider,
    ICarePlanRepository carePlanRepository,
    IDoctorRepository doctorRepository,
    ICarePlanReviewReminderNotifier notifier,
    ILogger<CarePlanReviewReminderDispatcher> logger) : ICarePlanReviewReminderDispatcher
{
    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var asOfDate = DateOnly.FromDateTime(nowUtc);
        var plans = await carePlanRepository.ListDueForReviewReminderAsync(
            asOfDate,
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

            if (!plan.IsDueForReviewReminder(asOfDate))
            {
                continue;
            }

            var doctor = await doctorRepository.GetByIdAsync(plan.DoctorId, ct);
            if (doctor is null)
            {
                logger.LogWarning(
                    "Skipping care plan review reminder for plan {CarePlanId}; doctor {DoctorId} was not found.",
                    plan.Id,
                    plan.DoctorId);
                continue;
            }

            await notifier.NotifyReviewDueAsync(
                doctor.UserId,
                plan.Id,
                plan.PatientId,
                plan.Condition,
                plan.NextReviewAt,
                ct);

            if (!plan.MarkReviewReminderSent(nowUtc))
            {
                continue;
            }

            await carePlanRepository.UpdateAsync(plan, ct);
            dispatched++;

            logger.LogInformation(
                "Dispatched care plan review reminder for plan {CarePlanId} to doctor {DoctorId}.",
                plan.Id,
                plan.DoctorId);
        }

        return dispatched;
    }
}
