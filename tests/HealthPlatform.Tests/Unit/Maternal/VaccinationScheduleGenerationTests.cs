using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Domain.Vaccinations;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class VaccinationScheduleGenerationTests
{
    [Fact]
    public void BuildRecommendedSchedule_includes_all_national_doses_for_newborn()
    {
        var dateOfBirth = new DateOnly(2026, 7, 10);
        var asOfDate = dateOfBirth;

        var schedule = ChildImmunizationSchedulePolicies.BuildRecommendedSchedule(dateOfBirth, asOfDate);

        Assert.Equal(16, schedule.Count);
        Assert.Contains(schedule, item => item.VaccineName == "BCG" && item.DaysFromBirth == 0);
        Assert.Contains(schedule, item => item.VaccineName == "Measles" && item.DaysFromBirth == 450);
    }

    [Fact]
    public void BuildRecommendedSchedule_excludes_past_doses_for_older_child()
    {
        var dateOfBirth = new DateOnly(2025, 7, 10);
        var asOfDate = new DateOnly(2026, 7, 10);

        var schedule = ChildImmunizationSchedulePolicies.BuildRecommendedSchedule(dateOfBirth, asOfDate);

        Assert.Equal(2, schedule.Count);
        Assert.All(schedule, item =>
            Assert.True(
                ChildImmunizationSchedulePolicies.ResolveRecommendedDate(dateOfBirth, item.DaysFromBirth) >= asOfDate));
    }

    [Fact]
    public async Task InitializeChildSchedule_persists_recommended_entries_with_matching_dates()
    {
        var childProfileId = Guid.CreateVersion7();
        var dateOfBirth = new DateOnly(2026, 1, 15);
        var createdAtUtc = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        IReadOnlyCollection<VaccinationScheduleEntry>? capturedEntries = null;

        var repository = new Mock<IVaccinationScheduleRepository>();
        repository
            .Setup(repo => repo.HasScheduleForChildAsync(childProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository
            .Setup(repo => repo.AddRangeAsync(It.IsAny<IReadOnlyCollection<VaccinationScheduleEntry>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<VaccinationScheduleEntry>, CancellationToken>((entries, _) => capturedEntries = entries)
            .Returns(Task.CompletedTask);

        var initializer = new VaccinationScheduleInitializer(repository.Object);
        await initializer.InitializeChildScheduleAsync(childProfileId, dateOfBirth, createdAtUtc, CancellationToken.None);

        var expectedItems = ChildImmunizationSchedulePolicies.BuildRecommendedSchedule(
            dateOfBirth,
            DateOnly.FromDateTime(createdAtUtc));

        Assert.NotNull(capturedEntries);
        Assert.Equal(expectedItems.Count, capturedEntries!.Count);
        Assert.All(capturedEntries, entry =>
        {
            Assert.Equal(childProfileId, entry.ChildProfileId);
            Assert.Contains(
                expectedItems,
                item =>
                    item.VaccineName == entry.VaccineName
                    && ChildImmunizationSchedulePolicies.ResolveRecommendedDate(dateOfBirth, item.DaysFromBirth)
                        == entry.RecommendedDate);
        });
    }

    [Fact]
    public async Task InitializeChildSchedule_skips_when_schedule_already_exists()
    {
        var childProfileId = Guid.CreateVersion7();
        var repository = new Mock<IVaccinationScheduleRepository>();
        repository
            .Setup(repo => repo.HasScheduleForChildAsync(childProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var initializer = new VaccinationScheduleInitializer(repository.Object);
        await initializer.InitializeChildScheduleAsync(
            childProfileId,
            new DateOnly(2026, 1, 1),
            DateTime.UtcNow,
            CancellationToken.None);

        repository.Verify(
            repo => repo.AddRangeAsync(It.IsAny<IReadOnlyCollection<VaccinationScheduleEntry>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
