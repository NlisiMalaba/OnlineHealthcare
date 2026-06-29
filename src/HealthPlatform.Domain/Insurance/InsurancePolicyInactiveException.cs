namespace HealthPlatform.Domain.Insurance;

public sealed class InsurancePolicyInactiveException : Exception
{
    public InsurancePolicyInactiveException()
        : base("Patient does not have an active insurance policy for the requested insurer.")
    {
    }
}
