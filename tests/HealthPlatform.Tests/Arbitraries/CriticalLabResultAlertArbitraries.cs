using FsCheck;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record CriticalLabResultAlertCase(
    string LabPartnerCode,
    string LabPartnerOrderReference,
    string TestCode,
    string FileName);

public static class CriticalLabResultAlertArbitraries
{
    public static Arbitrary<CriticalLabResultAlertCase> CriticalLabResultAlertCase() =>
        (from partnerCodeLength in Gen.Choose(3, 8)
         from orderRefLength in Gen.Choose(8, 16)
         from testCodeLength in Gen.Choose(3, 8)
         select new CriticalLabResultAlertCase(
             new string('L', partnerCodeLength),
             new string('R', orderRefLength),
             new string('T', testCodeLength),
             "critical-result.pdf"))
        .ToArbitrary();
}
