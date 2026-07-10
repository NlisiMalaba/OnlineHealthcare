using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.MentalHealth;

public sealed class MoodLogsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_endpoint_returns_created_mood_log()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var controller = new MoodLogsController(_host.Sender);
        var created = await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 3, Notes = "Calm" },
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(created.Result);
        var payload = Assert.IsType<MoodLogMutationResultDto>(createdResult.Value);
        Assert.Equal(3, payload.MoodLog.Rating);
        Assert.Equal(patient.Id, payload.MoodLog.PatientId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Mood Patient",
                null,
                $"mood-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }
}
