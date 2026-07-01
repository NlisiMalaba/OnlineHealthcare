namespace HealthPlatform.Application.Wellness;

public interface IMedicationDoseReminderDispatcher
{
  Task<int> DispatchDueRemindersAsync(CancellationToken ct);
}
