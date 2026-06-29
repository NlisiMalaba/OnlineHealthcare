using FsCheck;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record LowStockAlertUpdateCase(
    int PreviousQuantity,
    int NewQuantity,
    int LowStockThreshold,
    bool UseMarkOutOfStock);

public static class LowStockAlertArbitraries
{
    public static Arbitrary<LowStockAlertUpdateCase> LowStockAlertUpdateCase() =>
        (from threshold in Gen.Choose(0, 100)
         from previous in Gen.Choose(0, 500)
         from useMarkOutOfStock in Arb.Generate<bool>()
         from newQty in (useMarkOutOfStock
             ? Gen.Constant(0)
             : Gen.Choose(0, 500))
         where previous != newQty
         select new LowStockAlertUpdateCase(previous, newQty, threshold, useMarkOutOfStock))
        .ToArbitrary();
}
