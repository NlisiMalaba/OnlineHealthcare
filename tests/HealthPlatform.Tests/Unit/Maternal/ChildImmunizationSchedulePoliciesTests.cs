using HealthPlatform.Domain.Vaccinations;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class ChildImmunizationSchedulePoliciesTests
{
    [Fact]
    public void BuildRecommendedSchedule_returns_future_doses_from_date_of_birth()
    {
        var dateOfBirth = new DateOnly(2025, 1, 15);
        var asOfDate = new DateOnly(2025, 1, 15);

        var schedule = ChildImmunizationSchedulePolicies.BuildRecommendedSchedule(dateOfBirth, asOfDate);

        Assert.NotEmpty(schedule);
        Assert.Contains(schedule, item => item.VaccineName == "BCG");
        Assert.All(schedule, item =>
            Assert.True(
                ChildImmunizationSchedulePolicies.ResolveRecommendedDate(dateOfBirth, item.DaysFromBirth) >= asOfDate));
    }

    [Fact]
    public void IsDueForReminder_is_true_within_seven_day_window()
    {
        var asOfDate = new DateOnly(2026, 1, 1);
        var dueDate = asOfDate.AddDays(VaccinationReminderPolicies.ReminderLeadDays);

        Assert.True(VaccinationReminderPolicies.IsDueForReminder(dueDate, asOfDate));
        Assert.False(VaccinationReminderPolicies.IsDueForReminder(dueDate.AddDays(1), asOfDate));
    }
}
