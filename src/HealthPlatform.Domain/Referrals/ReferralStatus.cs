namespace HealthPlatform.Domain.Referrals;

public enum ReferralStatus
{
    Pending = 0,
    NeedsAdditionalInformation = 1,
    Accepted = 2,
    Declined = 3,
    Completed = 4
}
