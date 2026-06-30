namespace HealthPlatform.Domain.Payments.CreditLine;

public sealed class CreditLimitExceededException : Exception
{
    public CreditLimitExceededException()
        : base("The charge amount exceeds the patient's available credit line balance.")
    {
    }
}
