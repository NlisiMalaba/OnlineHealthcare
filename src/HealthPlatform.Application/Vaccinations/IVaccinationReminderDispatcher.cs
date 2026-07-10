namespace HealthPlatform.Application.Vaccinations;

public interface IVaccinationReminderDispatcher
{
    Task<int> DispatchDueRemindersAsync(CancellationToken ct);
}
