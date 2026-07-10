using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class ConsecutiveLowMoodPromptTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Three_consecutive_low_mood_logs_emit_mental_health_resource_prompt()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var controller = new MoodLogsController(_host.Sender);
        var baseTime = DateTime.UtcNow;

        for (var index = 0; index < 3; index++)
        {
            await controller.CreateAsync(
                new CreateMoodLogRequest
                {
                    Rating = 1,
                    LoggedAtUtc = baseTime.AddHours(-index)
                },
                CancellationToken.None);
        }

        Assert.Single(_host.ConsecutiveLowMoodPromptNotifier.Calls);
        Assert.Equal(patient.Id, _host.ConsecutiveLowMoodPromptNotifier.Calls[0].PatientId);
        Assert.Equal(patient.UserId, _host.ConsecutiveLowMoodPromptNotifier.Calls[0].PatientUserId);

        Assert.True(await _host.DbContext.ConsecutiveLowMoodPrompts.AnyAsync());
        Assert.True(await _host.DbContext.DomainEventOutbox
            .AnyAsync(entry => entry.EventType.Contains("ConsecutiveLowMoodDetectedDomainEvent")));
    }

    [Fact]
    public async Task Two_consecutive_low_mood_logs_do_not_emit_prompt()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var controller = new MoodLogsController(_host.Sender);

        await controller.CreateAsync(new CreateMoodLogRequest { Rating = 1 }, CancellationToken.None);
        await controller.CreateAsync(new CreateMoodLogRequest { Rating = 1 }, CancellationToken.None);

        Assert.Empty(_host.ConsecutiveLowMoodPromptNotifier.Calls);
        Assert.False(await _host.DbContext.ConsecutiveLowMoodPrompts.AnyAsync());
    }

    [Fact]
    public async Task Consecutive_low_mood_prompt_is_idempotent_for_same_triggering_log()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var controller = new MoodLogsController(_host.Sender);
        string? triggeringLogId = null;

        for (var index = 0; index < 3; index++)
        {
            var created = await controller.CreateAsync(new CreateMoodLogRequest { Rating = 1 }, CancellationToken.None);
            triggeringLogId = Assert.IsType<CreatedResult>(created.Result).Value is MoodLogMutationResultDto mutation
                ? mutation.MoodLog.Id
                : throw new InvalidOperationException("Expected mood log payload.");
        }

        Assert.Single(_host.ConsecutiveLowMoodPromptNotifier.Calls);

        await controller.UpdateAsync(
            triggeringLogId!,
            new UpdateMoodLogRequest { Rating = 1, Notes = "Still low" },
            CancellationToken.None);

        Assert.Single(_host.ConsecutiveLowMoodPromptNotifier.Calls);
        Assert.Single(await _host.DbContext.ConsecutiveLowMoodPrompts.ToListAsync());
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Consecutive Low Mood Patient",
                null,
                $"consecutive-low-mood-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }
}
