namespace HealthPlatform.Application.Appointments;

public static class AppointmentPolicies
{
    public static readonly TimeSpan ReminderLeadTime = TimeSpan.FromMinutes(30);

    public static readonly TimeSpan EarlyCancellationWindow = TimeSpan.FromHours(2);
}
