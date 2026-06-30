namespace HealthPlatform.Domain.Payments.Instalments;

public sealed class InstalmentExpenseBelowThresholdException : Exception
{
    public InstalmentExpenseBelowThresholdException(long minimumMinorUnits)
        : base($"Healthcare expense must be at least {minimumMinorUnits} minor units to qualify for an instalment plan.")
    {
    }
}

public sealed class InvalidInstalmentPlanException : Exception
{
    public InvalidInstalmentPlanException(string message)
        : base(message)
    {
    }
}
