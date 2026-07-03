namespace HealthPlatform.Application.NextOfKin;

public static class NextOfKinPolicies
{
    public const int MaxContactsPerPatient = 3;

    public const int MaxNotificationRetries = 3;

    public static readonly TimeSpan NotificationRetryInterval = TimeSpan.FromMinutes(5);

    public const int NotificationRetryBatchSize = 100;
}
