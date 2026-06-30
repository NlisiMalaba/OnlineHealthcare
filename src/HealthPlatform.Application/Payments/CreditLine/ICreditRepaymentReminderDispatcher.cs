namespace HealthPlatform.Application.Payments.CreditLine;

public interface ICreditRepaymentReminderDispatcher
{
    Task<int> DispatchDueRemindersAsync(CancellationToken ct);
}
