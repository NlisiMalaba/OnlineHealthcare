namespace HealthPlatform.Domain.Payments.CreditLine;

public static class CreditLinePolicies
{
    public const int BalanceWarningThresholdPercent = 80;

    public const int DefaultRepaymentDueDays = 30;

    public static bool ShouldEmitBalanceWarning(long outstandingBalanceMinorUnits, long creditLimitMinorUnits) =>
        creditLimitMinorUnits > 0
        && outstandingBalanceMinorUnits * 100 > creditLimitMinorUnits * BalanceWarningThresholdPercent;

    public static bool HasAvailableCredit(
        long outstandingBalanceMinorUnits,
        long creditLimitMinorUnits,
        long chargeAmountMinorUnits) =>
        creditLimitMinorUnits > 0
        && chargeAmountMinorUnits > 0
        && outstandingBalanceMinorUnits + chargeAmountMinorUnits <= creditLimitMinorUnits;
}
