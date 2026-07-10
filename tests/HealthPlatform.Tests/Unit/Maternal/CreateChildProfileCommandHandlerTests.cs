using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;
using HealthPlatform.Application.Maternal.ChildProfiles.GetChildProfile;
using HealthPlatform.Application.Maternal.ChildProfiles.ListChildProfiles;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class CreateChildProfileCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_child_profile_links_separate_health_record()
    {
        var guardian = await SeedGuardianAsync();
        _host.CurrentUser.UserId = guardian.UserId;
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3));

        var result = await _host.Sender.Send(
            new CreateChildProfileCommand(
                "Amina Okonkwo",
                dateOfBirth,
                "O+",
                ["Peanuts", "Penicillin"]),
            CancellationToken.None);

        Assert.Equal(guardian.Id, result.GuardianId);
        Assert.Equal("Amina Okonkwo", result.FullName);
        Assert.Equal(dateOfBirth, result.DateOfBirth);
        Assert.Equal("O+", result.BloodType);
        Assert.Equal(2, result.KnownAllergies.Count);
        Assert.NotEqual(Guid.Empty, result.HealthRecordId);

        var guardianHealthRecord = await _host.DbContext.HealthRecords
            .SingleAsync(record => record.PatientId == guardian.Id && record.ChildProfileId == null);
        var childHealthRecord = await _host.DbContext.HealthRecords
            .SingleAsync(record => record.Id == result.HealthRecordId);

        Assert.NotEqual(guardianHealthRecord.Id, childHealthRecord.Id);
        Assert.Equal(guardian.Id, childHealthRecord.PatientId);
        Assert.Equal(result.Id, childHealthRecord.ChildProfileId);

        var storedProfile = await _host.DbContext.ChildProfiles.SingleAsync(profile => profile.Id == result.Id);
        Assert.Equal(result.HealthRecordId, storedProfile.HealthRecordId);
    }

    [Fact]
    public async Task Create_child_profile_allows_multiple_children_per_guardian()
    {
        var guardian = await SeedGuardianAsync("multi-child");
        _host.CurrentUser.UserId = guardian.UserId;
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5));

        var first = await _host.Sender.Send(
            new CreateChildProfileCommand("Child One", dateOfBirth, null, []),
            CancellationToken.None);
        var second = await _host.Sender.Send(
            new CreateChildProfileCommand("Child Two", dateOfBirth.AddYears(-2), "A-", ["Dust"]),
            CancellationToken.None);

        Assert.NotEqual(first.Id, second.Id);
        Assert.NotEqual(first.HealthRecordId, second.HealthRecordId);

        var childRecords = await _host.DbContext.HealthRecords
            .Where(record => record.PatientId == guardian.Id && record.ChildProfileId != null)
            .ToListAsync();
        Assert.Equal(2, childRecords.Count);
    }

    [Fact]
    public async Task List_and_get_child_profiles_return_guardian_owned_profiles_only()
    {
        var guardian = await SeedGuardianAsync("list");
        var otherGuardian = await SeedGuardianAsync("other");
        _host.CurrentUser.UserId = guardian.UserId;
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2));

        var created = await _host.Sender.Send(
            new CreateChildProfileCommand("Listed Child", dateOfBirth, null, []),
            CancellationToken.None);

        _host.CurrentUser.UserId = otherGuardian.UserId;
        await _host.Sender.Send(
            new CreateChildProfileCommand("Other Child", dateOfBirth, null, []),
            CancellationToken.None);

        _host.CurrentUser.UserId = guardian.UserId;
        var listed = await _host.Sender.Send(new ListChildProfilesQuery(), CancellationToken.None);
        var fetched = await _host.Sender.Send(new GetChildProfileQuery(created.Id), CancellationToken.None);

        Assert.Single(listed);
        Assert.Equal(created.Id, listed[0].Id);
        Assert.Equal(created.Id, fetched.Id);

        _host.CurrentUser.UserId = otherGuardian.UserId;
        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            _host.Sender.Send(new GetChildProfileQuery(created.Id), CancellationToken.None));
    }

    private async Task<Patient> SeedGuardianAsync(string suffix = "guardian")
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
