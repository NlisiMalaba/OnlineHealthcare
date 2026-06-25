using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class CreatePrescriptionDrugInteractionTests : IAsyncLifetime
{
    private CapturingDrugInteractionAlertNotifier _alertNotifier = null!;
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _alertNotifier = new CapturingDrugInteractionAlertNotifier();
        _host = new PatientRegistrationTestHost(drugInteractionAlertNotifier: _alertNotifier);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_prescription_alerts_doctor_when_interaction_detected_before_finalization()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        await SeedActiveScheduleAsync(patient.Id, "Warfarin");

        _host.CurrentUser.UserId = doctor.UserId;
        var result = await _host.Sender.Send(
            new CreatePrescriptionCommand(
                patient.Id,
                "Ibuprofen",
                "400mg",
                "Every 8 hours",
                5,
                null,
                null,
                null),
            CancellationToken.None);

        Assert.Equal("active", result.Status);
        Assert.Single(_alertNotifier.Calls);
        Assert.Equal(doctor.UserId, _alertNotifier.Calls[0].DoctorUserId);
        Assert.Equal("Ibuprofen", _alertNotifier.Calls[0].ProposedMedicationName);
        Assert.Equal("Warfarin", _alertNotifier.Calls[0].InteractingMedicationName);

        Assert.True(await _host.DbContext.DomainEventOutbox
            .AnyAsync(x => x.EventType.Contains("DrugInteractionAlertDetectedDomainEvent")));
        Assert.True(await _host.DbContext.DomainEventOutbox
            .AnyAsync(x => x.EventType.Contains("PrescriptionIssuedDomainEvent")));
    }

    [Fact]
    public async Task Create_prescription_skips_alert_when_patient_has_no_active_schedule()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();

        _host.CurrentUser.UserId = doctor.UserId;
        await _host.Sender.Send(
            new CreatePrescriptionCommand(
                patient.Id,
                "Ibuprofen",
                "400mg",
                "Every 8 hours",
                5,
                null,
                null,
                null),
            CancellationToken.None);

        Assert.Empty(_alertNotifier.Calls);
    }

    private async Task<Doctor> SeedVerifiedDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Patient Interaction",
                null,
                $"patient-interaction-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }

    private async Task SeedActiveScheduleAsync(Guid patientId, string medicationName)
    {
        var scheduleRepository = _host.GetRequiredService<IMedicationScheduleRepository>();
        await scheduleRepository.AddAsync(
            MedicationSchedule.CreateActive(Guid.CreateVersion7(), patientId, medicationName),
            CancellationToken.None);
    }
}
