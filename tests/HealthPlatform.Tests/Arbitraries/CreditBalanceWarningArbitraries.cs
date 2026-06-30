using FsCheck;
using HealthPlatform.Domain.Payments.CreditLine;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record CreditBalanceWarningCase(
    long CreditLimitMinorUnits,
    long PreviousOutstandingMinorUnits,
    long ChargeAmountMinorUnits);

public static class CreditBalanceWarningArbitraries
{
    public static Arbitrary<CreditBalanceWarningCase> CreditBalanceWarningCase() =>
        (from limit in Gen.Choose(1000, 1_000_000).Select(i => (long)i)
         from previous in Gen.Choose(0, 1_000_000).Select(i => (long)i)
         from charge in Gen.Choose(1, 500_000).Select(i => (long)i)
         where previous + charge <= limit
         select new CreditBalanceWarningCase(limit, previous, charge))
        .ToArbitrary();
}
