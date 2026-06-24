using FsCheck;
using HealthPlatform.Domain.Prescriptions;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record PrescriptionDefaultExpiryCase(DateTime IssuedAtUtc, int DurationDays);

public enum PrescriptionDispensingScenarioKind
{
    Valid = 0,
    Expired = 1,
    WrongPatient = 2
}

public sealed record PrescriptionDispensingCase(
    PrescriptionDispensingScenarioKind Kind,
    int DaysSinceIssue,
    int DurationDays);

public static class PrescriptionArbitraries
{
    public static Arbitrary<PrescriptionDefaultExpiryCase> PrescriptionDefaultExpiryCase() =>
        (from issuedAt in Arb.Default.DateTime().Generator.Where(dt => dt != default)
         from durationDays in Gen.Choose(1, 365)
         select new PrescriptionDefaultExpiryCase(
             DateTime.SpecifyKind(issuedAt, DateTimeKind.Utc),
             durationDays)).ToArbitrary();

    public static Arbitrary<PrescriptionDispensingCase> PrescriptionDispensingCase() =>
        Gen.OneOf(ValidDispensingCase(), ExpiredDispensingCase(), WrongPatientDispensingCase())
            .ToArbitrary();

    private static Gen<PrescriptionDispensingCase> ValidDispensingCase() =>
        from daysSinceIssue in Gen.Choose(0, 25)
        from durationDays in Gen.Choose(1, 365)
        select new PrescriptionDispensingCase(
            PrescriptionDispensingScenarioKind.Valid,
            daysSinceIssue,
            durationDays);

    private static Gen<PrescriptionDispensingCase> ExpiredDispensingCase() =>
        from daysSinceIssue in Gen.Choose(31, 90)
        from durationDays in Gen.Choose(1, 365)
        select new PrescriptionDispensingCase(
            PrescriptionDispensingScenarioKind.Expired,
            daysSinceIssue,
            durationDays);

    private static Gen<PrescriptionDispensingCase> WrongPatientDispensingCase() =>
        from daysSinceIssue in Gen.Choose(0, 25)
        from durationDays in Gen.Choose(1, 365)
        select new PrescriptionDispensingCase(
            PrescriptionDispensingScenarioKind.WrongPatient,
            daysSinceIssue,
            durationDays);
}
