using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;
using HealthPlatform.Application.Maternal.GrowthEntries.GetChildGrowthChart;
using HealthPlatform.Application.Maternal.GrowthEntries.ListGrowthEntries;
using HealthPlatform.Application.Maternal.GrowthEntries.RecordGrowthEntry;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Maternal;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class ChildGrowthWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Record_growth_entry_returns_chart_with_reference_curves()
    {
        var guardian = await SeedGuardianAsync();
        _host.CurrentUser.UserId = guardian.UserId;
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));

        var profile = await _host.Sender.Send(
            new CreateChildProfileCommand("Growth Child", dateOfBirth, null, []),
            CancellationToken.None);

        await _host.Sender.Send(
            new RecordGrowthEntryCommand(profile.Id, 72m, 9.5m, null, null),
            CancellationToken.None);

        var chart = await _host.Sender.Send(new GetChildGrowthChartQuery(profile.Id), CancellationToken.None);

        Assert.NotEmpty(chart.HeightReferenceCurves);
        Assert.NotEmpty(chart.WeightReferenceCurves);
        Assert.Single(chart.Entries);
        Assert.False(chart.Entries[0].IsOutOfRange);
    }

    [Fact]
    public async Task Record_out_of_range_growth_entry_alerts_guardian()
    {
        var guardian = await SeedGuardianAsync("alert");
        _host.CurrentUser.UserId = guardian.UserId;
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));

        var profile = await _host.Sender.Send(
            new CreateChildProfileCommand("Alert Child", dateOfBirth, null, []),
            CancellationToken.None);

        var entry = await _host.Sender.Send(
            new RecordGrowthEntryCommand(profile.Id, 50m, null, null, null),
            CancellationToken.None);

        Assert.True(entry.IsOutOfRange);
        Assert.Equal(ChildGrowthMeasurementStatus.BelowRange, entry.HeightStatus);

        Assert.Single(_host.ChildGrowthOutOfRangeNotifier.Calls);
        var alert = _host.ChildGrowthOutOfRangeNotifier.Calls[0];
        Assert.Equal(guardian.UserId, alert.GuardianUserId);
        Assert.Equal(profile.Id, alert.ChildProfileId);
        Assert.Equal(entry.Id, alert.GrowthEntryId);

        var listed = await _host.Sender.Send(new ListGrowthEntriesQuery(profile.Id), CancellationToken.None);
        Assert.Single(listed);
        Assert.True(listed[0].IsOutOfRange);
    }

    [Fact]
    public async Task Record_milestone_only_entry_does_not_trigger_measurement_alert()
    {
        var guardian = await SeedGuardianAsync("milestone");
        _host.CurrentUser.UserId = guardian.UserId;
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6));

        var profile = await _host.Sender.Send(
            new CreateChildProfileCommand("Milestone Child", dateOfBirth, null, []),
            CancellationToken.None);

        var entry = await _host.Sender.Send(
            new RecordGrowthEntryCommand(profile.Id, null, null, "First steps", null),
            CancellationToken.None);

        Assert.False(entry.IsOutOfRange);
        Assert.Empty(_host.ChildGrowthOutOfRangeNotifier.Calls);
    }

    private async Task<Patient> SeedGuardianAsync(string suffix = "growth")
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Guardian {suffix}",
                null,
                $"guardian-{suffix}-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
