using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Payments.Instalments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Application.Payments.Instalments;

public sealed class InstalmentDueReminderDispatcher(
    IInstalmentPaymentRepository paymentRepository,
    IPatientRepository patientRepository,
    IInstalmentDueReminderNotifier reminderNotifier,
    TimeProvider timeProvider,
    ILogger<InstalmentDueReminderDispatcher> logger) : IInstalmentDueReminderDispatcher
{
    private const int BatchSize = 50;

    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var payments = await paymentRepository.ListDueRemindersAsync(nowUtc, BatchSize, ct);
        var dispatched = 0;

        foreach (var payment in payments)
        {
            if (!InstalmentPolicies.IsDueForReminder(payment.DueDate, nowUtc))
            {
                continue;
            }

            var patient = await patientRepository.GetByIdAsync(payment.PatientId, ct);
            if (patient is null)
            {
                logger.LogWarning(
                    "Skipping instalment reminder for missing patient {PatientId}.",
                    payment.PatientId);
                continue;
            }

            await reminderNotifier.NotifyDueReminderAsync(
                patient.UserId,
                patient.Id,
                payment.InstalmentPlanId,
                payment.Id,
                payment.SequenceNumber,
                payment.AmountMinorUnits,
                payment.Currency,
                payment.DueDate,
                ct);

            payment.MarkDueReminderSent();
            await paymentRepository.UpdateAsync(payment, ct);
            dispatched++;
        }

        if (dispatched > 0)
        {
            await paymentRepository.SaveChangesAsync(ct);
        }

        return dispatched;
    }
}

public sealed class InstalmentMissedPaymentProcessor(
    IInstalmentPaymentRepository paymentRepository,
    IInstalmentPlanRepository planRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    IOptions<InstalmentPlanOptions> options,
    TimeProvider timeProvider,
    ILogger<InstalmentMissedPaymentProcessor> logger) : IInstalmentMissedPaymentProcessor
{
    private const int BatchSize = 50;

    public async Task<int> ProcessMissedPaymentsAsync(CancellationToken ct)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var payments = await paymentRepository.ListMissedCandidatesAsync(nowUtc, BatchSize, ct);
        var processed = 0;
        var lateFeeMinorUnits = options.Value.LateFeeMinorUnits;

        foreach (var payment in payments)
        {
            if (!InstalmentPolicies.IsMissed(payment.DueDate, nowUtc))
            {
                continue;
            }

            var plan = await planRepository.GetByIdAsync(payment.InstalmentPlanId, ct);
            if (plan is null)
            {
                logger.LogWarning(
                    "Skipping missed instalment processing for missing plan {PlanId}.",
                    payment.InstalmentPlanId);
                continue;
            }

            var appliedLateFee = payment.MarkMissed(lateFeeMinorUnits, nowUtc);
            plan.ApplyLateFee(appliedLateFee);
            plan.MarkDefaulted();

            await paymentRepository.UpdateAsync(payment, ct);
            await planRepository.UpdateAsync(plan, ct);

            foreach (var domainEvent in payment.DomainEvents)
            {
                await outboxRepository.EnqueueAsync(domainEvent, ct);
                await domainEventPublisher.PublishAsync(domainEvent, ct);
            }

            payment.ClearDomainEvents();

            processed++;
        }

        if (processed > 0)
        {
            await paymentRepository.SaveChangesAsync(ct);
            await planRepository.SaveChangesAsync(ct);
        }

        return processed;
    }
}
