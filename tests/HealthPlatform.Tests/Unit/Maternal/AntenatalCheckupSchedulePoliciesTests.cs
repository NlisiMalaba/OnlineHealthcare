using HealthPlatform.Domain.Maternal;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class AntenatalCheckupSchedulePoliciesTests
{
    [Fact]
    public void BuildRecommendedSchedule_returns_future_checkups_from_current_gestational_age()
    {
        var dueDate = new DateOnly(2026, 12, 1);
        var asOfDate = new DateOnly(2026, 7, 10);
        var gestationalAgeWeeks = 12;

        var schedule = AntenatalCheckupSchedulePolicies.BuildRecommendedSchedule(
            gestationalAgeWeeks,
            dueDate,
            asOfDate);

        Assert.NotEmpty(schedule);
        Assert.All(schedule, item => Assert.True(item.GestationalAgeWeeks > gestationalAgeWeeks));
        Assert.All(schedule, item => Assert.True(item.RecommendedDate >= asOfDate));
        Assert.Equal(20, schedule[0].GestationalAgeWeeks);
    }

    [Fact]
    public void BuildRecommendedSchedule_returns_empty_when_pregnancy_is_near_term()
    {
        var dueDate = new DateOnly(2026, 7, 20);
        var asOfDate = new DateOnly(2026, 7, 10);

        var schedule = AntenatalCheckupSchedulePolicies.BuildRecommendedSchedule(
            40,
            dueDate,
            asOfDate);

        Assert.Empty(schedule);
    }
}
