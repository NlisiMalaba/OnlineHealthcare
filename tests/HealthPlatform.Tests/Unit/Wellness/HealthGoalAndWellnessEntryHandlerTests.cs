using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Wellness;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Wellness.HealthGoals;
using HealthPlatform.Application.Wellness.HealthGoals.CreateHealthGoal;
using HealthPlatform.Application.Wellness.WellnessEntries;
using HealthPlatform.Application.Wellness.WellnessEntries.RecordWellnessEntry;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class HealthGoalAndWellnessEntryHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;
    private FakeTimeProvider _clock = null!;

    public async Task InitializeAsync()
    {
        _clock = new FakeTimeProvider(new DateTime(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _clock);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task CreateHealthGoal_defaults_unit_for_standard_metric()
    {
        await RegisterAndSetCurrentPatientAsync();

        var goal = await _host.Sender.Send(
            new CreateHealthGoalCommand(WellnessMetricType.Steps, 10000m, null, null),
            CancellationToken.None);

        Assert.Equal(WellnessMetricType.Steps, goal.MetricType);
        Assert.Equal("steps", goal.Unit);
        Assert.Equal(HealthGoalStatus.Active, goal.Status);
        Assert.Equal(10000m, goal.TargetValue);
    }

    [Fact]
    public async Task RecordWellnessEntry_returns_progress_against_active_goals()
    {
        await RegisterAndSetCurrentPatientAsync();

        var goal = await _host.Sender.Send(
            new CreateHealthGoalCommand(WellnessMetricType.WaterMl, 2000m, null, null),
            CancellationToken.None);

        var entry = await _host.Sender.Send(
            new RecordWellnessEntryCommand(WellnessMetricType.WaterMl, 1500m, goal.Id, null),
            CancellationToken.None);

        Assert.Equal(goal.Id, entry.GoalId);
        Assert.Single(entry.GoalProgress);
        Assert.Equal(75m, entry.GoalProgress[0].ProgressPercent);
        Assert.False(entry.GoalProgress[0].IsAchieved);
        Assert.Equal(goal.Id, entry.GoalProgress[0].GoalId);
    }

    [Fact]
    public async Task RecordWellnessEntry_ignores_archived_goals_for_progress()
    {
        await RegisterAndSetCurrentPatientAsync();

        var goal = await _host.Sender.Send(
            new CreateHealthGoalCommand(WellnessMetricType.SleepHours, 8m, null, null),
            CancellationToken.None);

        var entity = await _host.DbContext.HealthGoals.FindAsync(goal.Id);
        Assert.NotNull(entity);
        entity.Archive();
        await _host.DbContext.SaveChangesAsync();

        var entry = await _host.Sender.Send(
            new RecordWellnessEntryCommand(WellnessMetricType.SleepHours, 7m, null, null),
            CancellationToken.None);

        Assert.Empty(entry.GoalProgress);
    }

    [Fact]
    public async Task HealthGoalsController_create_and_get_round_trip()
    {
        await RegisterAndSetCurrentPatientAsync();
        var controller = new HealthGoalsController(_host.Sender);

        var createdResult = await controller.CreateAsync(
            new CreateHealthGoalRequest
            {
                MetricType = WellnessMetricType.Weight,
                TargetValue = 70m
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(createdResult.Result);
        var goal = Assert.IsType<HealthGoalDto>(created.Value);

        var getResult = await controller.GetAsync(goal.Id, CancellationToken.None);
        var fetched = Assert.IsType<HealthGoalDto>(Assert.IsType<OkObjectResult>(getResult.Result).Value);
        Assert.Equal(goal.Id, fetched.Id);
        Assert.Equal("kg", fetched.Unit);
    }

    [Fact]
    public async Task WellnessEntriesController_record_returns_created_with_progress()
    {
        await RegisterAndSetCurrentPatientAsync();
        await _host.Sender.Send(
            new CreateHealthGoalCommand(WellnessMetricType.Steps, 8000m, null, null),
            CancellationToken.None);

        var controller = new WellnessEntriesController(_host.Sender);
        var result = await controller.RecordAsync(
            new RecordWellnessEntryRequest
            {
                MetricType = WellnessMetricType.Steps,
                Value = 4000m
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var entry = Assert.IsType<WellnessEntryDto>(created.Value);
        Assert.Single(entry.GoalProgress);
        Assert.Equal(50m, entry.GoalProgress[0].ProgressPercent);
    }

    private async Task RegisterAndSetCurrentPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Wellness Patient",
                null,
                $"wellness-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).First();
        _host.CurrentUser.UserId = patient.UserId;
    }
}
