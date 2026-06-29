namespace HealthPlatform.Domain.Insurance;

public sealed class DuplicateInsuranceClaimException : Exception
{
    public DuplicateInsuranceClaimException(string message)
        : base(message)
    {
    }
}
