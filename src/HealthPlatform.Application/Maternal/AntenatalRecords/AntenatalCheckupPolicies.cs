namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public static class AntenatalCheckupPolicies
{
    public const int MinGestationalAgeWeeks = 0;

    public const int MaxGestationalAgeWeeks = 42;

    public const int MinFetalHeartRateBpm = 60;

    public const int MaxFetalHeartRateBpm = 220;

    public const int MinBloodPressureSystolic = 70;

    public const int MaxBloodPressureSystolic = 250;

    public const int MinBloodPressureDiastolic = 40;

    public const int MaxBloodPressureDiastolic = 150;

    public const int MaxClinicalNotesLength = 4000;
}
