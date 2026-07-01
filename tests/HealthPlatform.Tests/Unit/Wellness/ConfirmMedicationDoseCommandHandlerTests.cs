using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Application.Wellness.ConfirmMedicationDose;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class ConfirmMedicationDoseCommandHandlerTests : IAsyncLifetime
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
    public async Task Handle_records_taken_adherence_event_for_confirmed_dose()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 24, 9, 30, 0, DateTimeKind.Utc);
        var (patient, schedule) = await SeedScheduleAsync(scheduledAtUtc);

        _host.CurrentUser.UserId = patient.UserId;
        var result = await _host.Sender.Send(
            new ConfirmMedicationDoseCommand(schedule.Id, scheduledAtUtc),
            CancellationToken.None);

        Assert.Equal("taken", result.Status);
        Assert.Equal(scheduledAtUtc, result.ScheduledAtUtc);
        Assert.Equal(_clock.GetUtcNow().UtcDateTime, result.RecordedAtUtc);

        var stored = await _host.DbContext.AdherenceEvents.SingleAsync();
        Assert.Equal(AdherenceEventStatus.Taken, stored.Status);
    }

    [Fact]
    public async Task Handle_rejects_confirmation_after_two_hour_window()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 24, 7, 0, 0, DateTimeKind.Utc);
        var (patient, schedule) = await SeedScheduleAsync(scheduledAtUtc);

        _host.CurrentUser.UserId = patient.UserId;
        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new ConfirmMedicationDoseCommand(schedule.Id, scheduledAtUtc),
            CancellationToken.None));

        Assert.Equal(WellnessErrorCodes.DoseConfirmationWindowExpired, ex.Code);
    }

    [Fact]
    public async Task Handle_rejects_duplicate_confirmation()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 24, 9, 30, 0, DateTimeKind.Utc);
        var (patient, schedule) = await SeedScheduleAsync(scheduledAtUtc);

        _host.CurrentUser.UserId = patient.UserId;
        await _host.Sender.Send(
            new ConfirmMedicationDoseCommand(schedule.Id, scheduledAtUtc),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _host.Sender.Send(
            new ConfirmMedicationDoseCommand(schedule.Id, scheduledAtUtc),
            CancellationToken.None));

        Assert.Equal(WellnessErrorCodes.DoseAlreadyRecorded, ex.Code);
    }

    private async Task<(Patient Patient, MedicationSchedule Schedule)> SeedScheduleAsync(DateTime doseAtUtc)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Adherence Patient",
                null,
                $"adherence-{Guid.NewGuid():N}@example.com",
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
        return (patient, schedule);
    }
}
