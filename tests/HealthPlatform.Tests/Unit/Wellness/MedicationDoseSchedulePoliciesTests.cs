using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Domain.Prescriptions.Events;
using HealthPlatform.Domain.Wellness;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class MedicationDoseSchedulePoliciesTests
{
  [Theory]
  [InlineData("Twice daily", 7, 14)]
  [InlineData("twice daily", 3, 6)]
  [InlineData("BID", 5, 10)]
  [InlineData("Once daily", 10, 10)]
  [InlineData("Three times daily", 2, 6)]
  [InlineData("Four times daily", 1, 4)]
  [InlineData("Every 8 hours", 2, 6)]
  public void BuildDoseTimes_generates_expected_dose_count(
      string frequency,
      int durationDays,
      int expectedDoseCount)
  {
    var dispensedAtUtc = new DateTime(2026, 6, 24, 7, 0, 0, DateTimeKind.Utc);

    var doseTimes = MedicationDoseSchedulePolicies.BuildDoseTimes(
        frequency,
        durationDays,
        dispensedAtUtc);

    Assert.Equal(expectedDoseCount, doseTimes.Count);
    Assert.Equal(doseTimes, doseTimes.OrderBy(doseTime => doseTime).ToList());
    Assert.All(doseTimes, doseTime => Assert.Equal(DateTimeKind.Utc, doseTime.Kind));
  }

  [Fact]
  public void BuildDoseTimes_skips_past_doses_on_dispense_day()
  {
    var dispensedAtUtc = new DateTime(2026, 6, 24, 15, 0, 0, DateTimeKind.Utc);

    var doseTimes = MedicationDoseSchedulePolicies.BuildDoseTimes(
        "Twice daily",
        2,
        dispensedAtUtc);

    Assert.Equal(4, doseTimes.Count);
    Assert.Equal(new DateTime(2026, 6, 24, 20, 0, 0, DateTimeKind.Utc), doseTimes[0]);
    Assert.Equal(new DateTime(2026, 6, 25, 8, 0, 0, DateTimeKind.Utc), doseTimes[1]);
    Assert.Equal(new DateTime(2026, 6, 25, 20, 0, 0, DateTimeKind.Utc), doseTimes[2]);
    Assert.Equal(new DateTime(2026, 6, 26, 8, 0, 0, DateTimeKind.Utc), doseTimes[3]);
  }

  [Fact]
  public void BuildDoseTimes_rejects_unsupported_frequency()
  {
    var dispensedAtUtc = new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Utc);

    Assert.Throws<InvalidMedicationFrequencyException>(() =>
        MedicationDoseSchedulePolicies.BuildDoseTimes(
            "As needed",
            7,
            dispensedAtUtc));
  }
}
