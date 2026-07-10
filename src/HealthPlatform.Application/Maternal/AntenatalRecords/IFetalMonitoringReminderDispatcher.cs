namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public interface IFetalMonitoringReminderDispatcher
{
    Task<int> DispatchDueRemindersAsync(CancellationToken ct);
}
