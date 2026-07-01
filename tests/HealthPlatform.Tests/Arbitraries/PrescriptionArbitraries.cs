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

public enum DrugInteractionFinalizationScenario
{
    InteractingSchedule = 0,
    NonInteractingSchedule = 1,
    EmptySchedule = 2
}

public sealed record DrugInteractionFinalizationCase(
    DrugInteractionFinalizationScenario Scenario,
    int DurationDays);

public sealed record MedicationScheduleGenerationCase(
    string Frequency,
    int DurationDays,
    int DispenseHourUtc,
    int DispenseMinuteUtc);

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

    public static Arbitrary<DrugInteractionFinalizationCase> DrugInteractionFinalizationCase() =>
        Gen.OneOf(InteractingScheduleCase(), NonInteractingScheduleCase(), EmptyScheduleCase())
            .ToArbitrary();

    private static Gen<DrugInteractionFinalizationCase> InteractingScheduleCase() =>
        from durationDays in Gen.Choose(1, 365)
        select new DrugInteractionFinalizationCase(
            DrugInteractionFinalizationScenario.InteractingSchedule,
            durationDays);

    private static Gen<DrugInteractionFinalizationCase> NonInteractingScheduleCase() =>
        from durationDays in Gen.Choose(1, 365)
        select new DrugInteractionFinalizationCase(
            DrugInteractionFinalizationScenario.NonInteractingSchedule,
            durationDays);

    private static Gen<DrugInteractionFinalizationCase> EmptyScheduleCase() =>
        from durationDays in Gen.Choose(1, 365)
        select new DrugInteractionFinalizationCase(
            DrugInteractionFinalizationScenario.EmptySchedule,
            durationDays);

    public static Arbitrary<MedicationScheduleGenerationCase> MedicationScheduleGenerationCase() =>
        (from frequency in Gen.Elements(
                "Once daily",
                "Twice daily",
                "Three times daily",
                "Four times daily",
                "BID",
                "TID",
                "Every 6 hours",
                "Every 8 hours",
                "Every 12 hours")
         from durationDays in Gen.Choose(1, 30)
         from dispenseHour in Gen.Choose(0, 23)
         from dispenseMinute in Gen.Choose(0, 59)
         select new MedicationScheduleGenerationCase(
             frequency,
             durationDays,
             dispenseHour,
             dispenseMinute)).ToArbitrary();
}
