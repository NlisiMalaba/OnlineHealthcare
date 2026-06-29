namespace HealthPlatform.Domain.Insurance;

public enum InsuranceClaimStatus
{
    Draft = 0,
    Submitted = 1,
    Processing = 2,
    Approved = 3,
    Rejected = 4,
    Paid = 5
}
