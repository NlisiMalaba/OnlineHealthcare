namespace HealthPlatform.Domain.Payments;

public static class PaymentPolicies
{
    public const int PendingRetentionMinutes = 10;

    public static TimeSpan PendingRetentionWindow { get; } = TimeSpan.FromMinutes(PendingRetentionMinutes);
}
