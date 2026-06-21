using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class PatientProfileUpdateWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task UpdateProfile_PersistsFieldsAndHealthRecordChanges()
    {
        var registration = await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Original Name",
                null,
                $"profile-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync(p => p.Id == registration.PatientId);
        _host.CurrentUser.UserId = patient.UserId;

        var updated = await _host.Sender.Send(
            new UpdatePatientProfileCommand(
                "Updated Name",
                new DateOnly(1992, 3, 10),
                BloodType.APositive,
                ["Peanuts", "Shellfish"],
                ["Hypertension"],
                null),
            CancellationToken.None);

        Assert.Equal("Updated Name", updated.FullName);
        Assert.Equal(new DateOnly(1992, 3, 10), updated.DateOfBirth);
        Assert.Equal(BloodType.APositive, updated.BloodType);
        Assert.Equal(["Peanuts", "Shellfish"], updated.KnownAllergies);
        Assert.Equal(["Hypertension"], updated.ChronicConditions);

        var changes = await _host.DbContext.HealthRecordProfileChanges
            .Where(c => c.PatientId == patient.Id)
            .ToListAsync();

        Assert.True(changes.Count >= 5);
        Assert.All(changes, change => Assert.True(change.ChangedAtUtc <= DateTime.UtcNow));
    }

    [Fact]
    public async Task UpdateProfile_WithPhoto_StoresPhotoKeyAndAuditEntry()
    {
        var registration = await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Phone,
                "Photo Patient",
                $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}",
                null,
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync(p => p.Id == registration.PatientId);
        _host.CurrentUser.UserId = patient.UserId;

        await using var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xD9]);
        var photo = new ProfilePhotoUpload(stream, "image/jpeg", "profile.jpg", stream.Length);

        var updated = await _host.Sender.Send(
            new UpdatePatientProfileCommand(null, null, null, null, null, photo),
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(updated.ProfilePhotoUrl));

        var storedPatient = await _host.DbContext.Patients.SingleAsync(p => p.Id == patient.Id);
        Assert.False(string.IsNullOrWhiteSpace(storedPatient.ProfilePhotoStorageKey));

        var photoChange = await _host.DbContext.HealthRecordProfileChanges
            .SingleAsync(c => c.PatientId == patient.Id && c.FieldName == nameof(Patient.ProfilePhotoStorageKey));

        Assert.NotNull(photoChange.NewValue);
    }
}
