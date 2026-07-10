using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Maternal;

public sealed class ChildProfilesControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_returns_created_child_profile_with_linked_health_record()
    {
        var guardian = await SeedGuardianAsync();
        _host.CurrentUser.UserId = guardian.UserId;
        var controller = new ChildProfilesController(_host.Sender);
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-4));

        var result = await controller.CreateAsync(
            new CreateChildProfileRequest(
                "Controller Child",
                dateOfBirth,
                "AB+",
                ["Latex"]),
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<ChildProfileDto>(created.Value);
        Assert.Equal(guardian.Id, dto.GuardianId);
        Assert.NotEqual(Guid.Empty, dto.HealthRecordId);
        Assert.Equal($"/api/v1/maternal/child-profiles/{dto.Id}", created.Location);

        var healthRecord = await _host.DbContext.HealthRecords
            .SingleAsync(record => record.Id == dto.HealthRecordId);
        Assert.Equal(dto.Id, healthRecord.ChildProfileId);
    }

    private async Task<Patient> SeedGuardianAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Controller Guardian",
                null,
                $"child-controller-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
