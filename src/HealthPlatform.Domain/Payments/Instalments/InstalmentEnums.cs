namespace HealthPlatform.Domain.Payments.Instalments;

public enum InstalmentFrequency
{
    Weekly = 0,
    Biweekly = 1,
    Monthly = 2
}

public enum InstalmentPlanStatus
{
    Active = 0,
    Completed = 1,
    Defaulted = 2
}

public enum InstalmentPaymentStatus
{
    Scheduled = 0,
    Paid = 1,
    Missed = 2
}
