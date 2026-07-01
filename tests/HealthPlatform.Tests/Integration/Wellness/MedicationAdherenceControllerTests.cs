using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Wellness;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Wellness;

public sealed class MedicationAdherenceControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;
    private FakeTimeProvider _clock = null!;

    public async Task InitializeAsync()
    {
        _clock = new FakeTimeProvider(new DateTime(2026, 6, 24, 10, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _clock);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task ConfirmDoseAsync_returns_created_adherence_event()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 24, 9, 30, 0, DateTimeKind.Utc);
        var (patientUserId, scheduleId) = await SeedScheduleAsync(scheduledAtUtc);
        _host.CurrentUser.UserId = patientUserId;

        var controller = new MedicationAdherenceController(_host.Sender);
        var result = await controller.ConfirmDoseAsync(
            scheduleId,
            new ConfirmMedicationDoseRequest { ScheduledAtUtc = scheduledAtUtc },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var adherenceEvent = Assert.IsType<AdherenceEventDto>(created.Value);
        Assert.Equal("taken", adherenceEvent.Status);
        Assert.Equal(scheduledAtUtc, adherenceEvent.ScheduledAtUtc);
    }

    private async Task<(Guid PatientUserId, Guid ScheduleId)> SeedScheduleAsync(DateTime doseAtUtc)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Controller Adherence Patient",
                null,
                $"controller-adherence-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        var schedule = MedicationSchedule.CreateActive(
            Guid.CreateVersion7(),
            patient.Id,
            "Amoxicillin",
            [doseAtUtc]);

        await _host.GetRequiredService<IMedicationScheduleRepository>().AddAsync(schedule, CancellationToken.None);
        return (patient.UserId, schedule.Id);
    }
}
