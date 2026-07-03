using FsCheck;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Tests.Arbitraries;

public enum HealthRecordAccessGrantState
{
    None = 0,
    Active = 1,
    Revoked = 2
}

public sealed record HealthRecordAccessCase(
    HealthRecordAccessGrantState GrantState,
    HealthRecordAccessType AccessType);

public static class HealthRecordAccessArbitraries
{
    public static Arbitrary<HealthRecordAccessCase> HealthRecordAccessCase() =>
        (from grantState in Gen.Elements(
                HealthRecordAccessGrantState.None,
                HealthRecordAccessGrantState.Active,
                HealthRecordAccessGrantState.Revoked)
         from accessType in Gen.Elements(
                HealthRecordAccessType.Full,
                HealthRecordAccessType.ReadOnly)
         select new HealthRecordAccessCase(grantState, accessType))
        .ToArbitrary();
}
