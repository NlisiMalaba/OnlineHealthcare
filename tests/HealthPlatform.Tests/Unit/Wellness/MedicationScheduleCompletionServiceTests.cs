using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Application.Wellness.ConfirmMedicationDose;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class MedicationScheduleCompletionServiceTests : IAsyncLifetime
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
    public async Task Confirming_final_dose_completes_schedule_and_notifies_patient_and_doctor()
    {
        var context = await SeedScheduleWithPrescriptionAsync(
            new DateTime(2026, 6, 24, 8, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 24, 9, 30, 0, DateTimeKind.Utc));

        _host.CurrentUser.UserId = context.Patient.UserId;
        await _host.Sender.Send(
            new ConfirmMedicationDoseCommand(
                context.Schedule.Id,
                new DateTime(2026, 6, 24, 8, 30, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        Assert.Empty(_host.MedicationScheduleCompletionNotifier.Calls);

        await _host.Sender.Send(
            new ConfirmMedicationDoseCommand(
                context.Schedule.Id,
                new DateTime(2026, 6, 24, 9, 30, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        var stored = await _host.DbContext.MedicationSchedules.AsNoTracking()
            .SingleAsync(s => s.Id == context.Schedule.Id);
        Assert.Equal(MedicationScheduleStatus.Completed, stored.Status);
        Assert.Equal(_clock.GetUtcNow().UtcDateTime, stored.CompletedAtUtc);

        var call = Assert.Single(_host.MedicationScheduleCompletionNotifier.Calls);
        Assert.Equal(context.Patient.UserId, call.PatientUserId);
        Assert.Equal(context.Doctor.UserId, call.DoctorUserId);
        Assert.Equal(context.Schedule.Id, call.ScheduleId);

        Assert.True(await _host.DbContext.DomainEventOutbox
            .AnyAsync(entry => entry.EventType.Contains("MedicationScheduleCompletedDomainEvent")));
    }

    [Fact]
    public async Task Completion_is_idempotent_when_evaluated_again()
    {
        var context = await SeedScheduleWithPrescriptionAsync(
            new DateTime(2026, 6, 24, 8, 30, 0, DateTimeKind.Utc));

        _host.CurrentUser.UserId = context.Patient.UserId;
        await _host.Sender.Send(
            new ConfirmMedicationDoseCommand(
                context.Schedule.Id,
                new DateTime(2026, 6, 24, 8, 30, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        await _host.GetRequiredService<IMedicationScheduleCompletionService>()
            .EvaluateCompletionAsync(context.Schedule.Id, CancellationToken.None);

        Assert.Single(_host.MedicationScheduleCompletionNotifier.Calls);
    }

    private async Task<ScheduleContext> SeedScheduleWithPrescriptionAsync(params DateTime[] doseTimes)
    {
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId), CancellationToken.None);
        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Completion Patient",
                null,
                $"completion-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);
        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();

        _host.CurrentUser.UserId = doctor.UserId;
        PrescriptionDto prescription = await _host.Sender.Send(
            new CreatePrescriptionCommand(
                patient.Id,
                "Amoxicillin",
                "500mg",
                "once daily",
                5,
                null,
                null,
                null),
            CancellationToken.None);

        var schedule = MedicationSchedule.CreateActive(
            prescription.Id,
            patient.Id,
            "Amoxicillin",
            doseTimes);
        await _host.GetRequiredService<IMedicationScheduleRepository>().AddAsync(schedule, CancellationToken.None);

        return new ScheduleContext(doctor, patient, schedule);
    }

    private sealed record ScheduleContext(Doctor Doctor, Patient Patient, MedicationSchedule Schedule);
}
