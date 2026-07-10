using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Maternal;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class CreateAntenatalRecordCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_antenatal_record_generates_schedule_and_notifies_patient()
    {
        var obstetricDoctor = await SeedVerifiedObstetricDoctorAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(120));

        var result = await _host.Sender.Send(
            new CreateAntenatalRecordCommand(dueDate, 12, obstetricDoctor.Id),
            CancellationToken.None);

        Assert.Equal(patient.Id, result.PatientId);
        Assert.Equal(obstetricDoctor.Id, result.ObstetricDoctorId);
        Assert.Equal(dueDate, result.EstimatedDueDate);
        Assert.Equal("active", result.Status);
        Assert.NotEmpty(result.RecommendedCheckups);
        Assert.NotNull(result.NextReminderAtUtc);

        Assert.Single(_host.AntenatalRecordCreatedNotifier.Calls);
        var notification = _host.AntenatalRecordCreatedNotifier.Calls[0];
        Assert.Equal(patient.UserId, notification.PatientUserId);
        Assert.Equal(obstetricDoctor.UserId, notification.ObstetricDoctorUserId);
        Assert.Equal(result.RecommendedCheckups.Count, notification.RecommendedCheckupCount);

        var storedEntries = await _host.DbContext.AntenatalCheckupScheduleEntries
            .Where(entry => entry.AntenatalRecordId == result.Id)
            .ToListAsync();
        Assert.Equal(result.RecommendedCheckups.Count, storedEntries.Count);
    }

    [Fact]
    public async Task Create_antenatal_record_sets_high_frequency_next_reminder_when_due_within_four_weeks()
    {
        var obstetricDoctor = await SeedVerifiedObstetricDoctorAsync("near-term");
        var patient = await SeedPatientAsync("near-term");
        _host.CurrentUser.UserId = patient.UserId;
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20));

        var result = await _host.Sender.Send(
            new CreateAntenatalRecordCommand(dueDate, 36, obstetricDoctor.Id),
            CancellationToken.None);

        Assert.NotNull(result.NextReminderAtUtc);
        var expectedNextReminder = DateTime.UtcNow.AddDays(AntenatalReminderPolicies.HighFrequencyReminderIntervalDays);
        Assert.True(result.NextReminderAtUtc <= expectedNextReminder.AddMinutes(1));
    }

    [Fact]
    public async Task Create_antenatal_record_rejects_non_obstetric_doctor()
    {
        var generalDoctor = await SeedVerifiedGeneralDoctorAsync();
        var patient = await SeedPatientAsync("non-obstetric");
        _host.CurrentUser.UserId = patient.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new CreateAntenatalRecordCommand(
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(120)),
                12,
                generalDoctor.Id),
            CancellationToken.None));

        Assert.Equal("DOCTOR_NOT_OBSTETRICIAN", ex.Code);
    }

    [Fact]
    public async Task Create_antenatal_record_rejects_second_active_record()
    {
        var obstetricDoctor = await SeedVerifiedObstetricDoctorAsync("duplicate");
        var patient = await SeedPatientAsync("duplicate");
        _host.CurrentUser.UserId = patient.UserId;
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(120));

        await _host.Sender.Send(
            new CreateAntenatalRecordCommand(dueDate, 12, obstetricDoctor.Id),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _host.Sender.Send(
            new CreateAntenatalRecordCommand(dueDate, 14, obstetricDoctor.Id),
            CancellationToken.None));

        Assert.Equal("ACTIVE_ANTENATAL_RECORD_EXISTS", ex.Code);
    }

    private async Task<Doctor> SeedVerifiedObstetricDoctorAsync(string suffix = "obstetric")
    {
        var registration = await _host.Sender.Send(
            ObstetricDoctorRegistrationTestData.CreateValidCommand(
                $"obstetric-{suffix}-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Doctor> SeedVerifiedGeneralDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(
                $"general-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync(string suffix = "default")
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Patient {suffix}",
                null,
                $"patient-{suffix}-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
