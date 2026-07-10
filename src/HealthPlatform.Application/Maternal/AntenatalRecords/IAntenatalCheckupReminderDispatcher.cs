namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public interface IAntenatalCheckupReminderDispatcher
{
    Task<int> DispatchDueRemindersAsync(CancellationToken ct);
}
