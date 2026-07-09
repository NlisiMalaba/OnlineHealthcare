namespace HealthPlatform.Application.Queue;

public static class QueuePolicies
{
    public const int DefaultConsultationDurationMinutes = 15;
    public static readonly TimeSpan RealtimeUpdateInterval = TimeSpan.FromMinutes(2);
}
