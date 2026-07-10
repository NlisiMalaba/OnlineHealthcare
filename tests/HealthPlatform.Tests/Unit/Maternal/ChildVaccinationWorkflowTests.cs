using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;
using HealthPlatform.Application.Vaccinations.ListChildVaccinationRecords;
using HealthPlatform.Application.Vaccinations.ListChildVaccinationSchedule;
using HealthPlatform.Application.Vaccinations.RecordChildVaccination;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class ChildVaccinationWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_child_profile_generates_vaccination_schedule()
    {
        var guardian = await SeedGuardianAsync();
        _host.CurrentUser.UserId = guardian.UserId;
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));

        var profile = await _host.Sender.Send(
            new CreateChildProfileCommand("Vaccine Child", dateOfBirth, null, []),
            CancellationToken.None);

        var schedule = await _host.Sender.Send(
            new ListChildVaccinationScheduleQuery(profile.Id),
            CancellationToken.None);

        Assert.NotEmpty(schedule);
        Assert.All(schedule, entry => Assert.False(entry.IsCompleted));
    }

    [Fact]
    public async Task Record_child_vaccination_completes_schedule_entry()
    {
        var guardian = await SeedGuardianAsync("record");
        _host.CurrentUser.UserId = guardian.UserId;
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));

        var profile = await _host.Sender.Send(
            new CreateChildProfileCommand("Recorded Child", dateOfBirth, null, []),
            CancellationToken.None);

        var schedule = await _host.Sender.Send(
            new ListChildVaccinationScheduleQuery(profile.Id),
            CancellationToken.None);
        var targetEntry = schedule.First();

        var record = await _host.Sender.Send(
            new RecordChildVaccinationCommand(
                profile.Id,
                targetEntry.Id,
                targetEntry.VaccineName,
                DateOnly.FromDateTime(DateTime.UtcNow),
                "BATCH-001",
                "Community Clinic"),
            CancellationToken.None);

        Assert.Equal(profile.Id, record.ChildProfileId);
        Assert.Equal(targetEntry.Id, record.ScheduleEntryId);

        var records = await _host.Sender.Send(
            new ListChildVaccinationRecordsQuery(profile.Id),
            CancellationToken.None);
        Assert.Single(records);

        var updatedSchedule = await _host.DbContext.VaccinationScheduleEntries
            .SingleAsync(entry => entry.Id == targetEntry.Id);
        Assert.NotNull(updatedSchedule.CompletedAtUtc);
    }

    private async Task<Patient> SeedGuardianAsync(string suffix = "vaccination")
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
