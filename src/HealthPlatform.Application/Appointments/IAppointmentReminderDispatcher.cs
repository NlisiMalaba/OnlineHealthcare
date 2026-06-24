namespace HealthPlatform.Application.Appointments;

public interface IAppointmentReminderDispatcher
{
    Task<int> DispatchDueRemindersAsync(CancellationToken ct);
}
