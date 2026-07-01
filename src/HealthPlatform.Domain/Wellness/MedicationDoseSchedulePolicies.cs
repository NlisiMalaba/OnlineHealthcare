using System.Text.RegularExpressions;

namespace HealthPlatform.Domain.Wellness;

public static partial class MedicationDoseSchedulePolicies
{
    private static readonly TimeOnly MorningDose = new(8, 0);
    private static readonly TimeOnly MiddayDose = new(12, 0);
    private static readonly TimeOnly AfternoonDose = new(14, 0);
    private static readonly TimeOnly EveningDose = new(16, 0);
    private static readonly TimeOnly NightDose = new(20, 0);

    public static IReadOnlyList<DateTime> BuildDoseTimes(
        string frequency,
        int durationDays,
        DateTime dispensedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(frequency))
        {
            throw new ArgumentException("Frequency is required.", nameof(frequency));
        }

        if (durationDays <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero.", nameof(durationDays));
        }

        if (dispensedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Dispense time must be UTC.", nameof(dispensedAtUtc));
        }

        var dosesPerDay = ParseDosesPerDay(frequency);
        var everyHoursMatch = EveryHoursFrequencyRegex().Match(frequency.Trim().ToLowerInvariant());
        if (everyHoursMatch.Success
            && int.TryParse(everyHoursMatch.Groups["hours"].Value, out var intervalHours)
            && intervalHours is > 0 and <= 24
            && 24 % intervalHours == 0)
        {
            return BuildIntervalDoseTimes(intervalHours, durationDays, dispensedAtUtc);
        }

        var anchorTimes = GetAnchorTimes(dosesPerDay);
        var scheduleEndUtc = dispensedAtUtc.AddDays(durationDays);
        var doseTimes = new List<DateTime>(dosesPerDay * durationDays);
        var date = DateOnly.FromDateTime(dispensedAtUtc);
        var lastDate = DateOnly.FromDateTime(scheduleEndUtc);

        while (date <= lastDate)
        {
            foreach (var anchorTime in anchorTimes)
            {
                var doseAt = date.ToDateTime(anchorTime, DateTimeKind.Utc);
                if (doseAt < dispensedAtUtc || doseAt >= scheduleEndUtc)
                {
                    continue;
                }

                doseTimes.Add(doseAt);
            }

            date = date.AddDays(1);
        }

        if (doseTimes.Count == 0)
        {
            throw new InvalidMedicationFrequencyException(frequency);
        }

        return doseTimes;
    }

    private static IReadOnlyList<DateTime> BuildIntervalDoseTimes(
        int intervalHours,
        int durationDays,
        DateTime dispensedAtUtc)
    {
        var doseTimes = new List<DateTime>();
        var interval = TimeSpan.FromHours(intervalHours);
        var scheduleEndUtc = dispensedAtUtc.AddDays(durationDays);
        var doseAt = AlignToNextInterval(dispensedAtUtc, interval);

        while (doseAt < scheduleEndUtc)
        {
            doseTimes.Add(doseAt);
            doseAt += interval;
        }

        if (doseTimes.Count == 0)
        {
            throw new InvalidMedicationFrequencyException($"Every {intervalHours} hours");
        }

        return doseTimes;
    }

    private static DateTime AlignToNextInterval(DateTime dispensedAtUtc, TimeSpan interval)
    {
        var startOfDay = DateOnly.FromDateTime(dispensedAtUtc)
            .ToDateTime(MorningDose, DateTimeKind.Utc);
        var doseAt = startOfDay;

        while (doseAt < dispensedAtUtc)
        {
            doseAt += interval;
        }

        return doseAt;
    }

    private static IReadOnlyList<TimeOnly> GetAnchorTimes(int dosesPerDay) =>
        dosesPerDay switch
        {
            1 => [MorningDose],
            2 => [MorningDose, NightDose],
            3 => [MorningDose, AfternoonDose, NightDose],
            4 => [MorningDose, MiddayDose, EveningDose, NightDose],
            _ => throw new ArgumentOutOfRangeException(nameof(dosesPerDay), dosesPerDay, "Unsupported doses per day.")
        };

    private static int ParseDosesPerDay(string frequency)
    {
        var normalized = frequency.Trim().ToLowerInvariant();

        if (normalized is "once daily" or "one time daily" or "daily" or "od" or "qd")
        {
            return 1;
        }

        if (normalized is "twice daily" or "two times daily" or "bid")
        {
            return 2;
        }

        if (normalized is "three times daily" or "tid")
        {
            return 3;
        }

        if (normalized is "four times daily" or "qid")
        {
            return 4;
        }

        var everyHoursMatch = EveryHoursFrequencyRegex().Match(normalized);
        if (everyHoursMatch.Success
            && int.TryParse(everyHoursMatch.Groups["hours"].Value, out var intervalHours)
            && intervalHours is > 0 and <= 24
            && 24 % intervalHours == 0)
        {
            return 24 / intervalHours;
        }

        throw new InvalidMedicationFrequencyException(frequency);
    }

    [GeneratedRegex(@"every\s+(?<hours>\d+)\s*hours?", RegexOptions.IgnoreCase)]
    private static partial Regex EveryHoursFrequencyRegex();
}
