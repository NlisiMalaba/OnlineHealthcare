using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Prescriptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Prescriptions;

public sealed class PrescriptionsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_endpoint_issues_prescription_with_default_expiry()
    {
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(
            new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == doctorRegistration.DoctorId);
        _host.CurrentUser.UserId = doctor.UserId;

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Patient Rx",
                null,
                $"patient-rx-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();

        var controller = new PrescriptionsController(_host.Sender);
        var result = await controller.CreateAsync(
            new CreatePrescriptionRequest
            {
                PatientId = patient.Id,
                MedicationName = "Paracetamol",
                Dosage = "500mg",
                Frequency = "Every 8 hours",
                DurationDays = 5,
                SpecialInstructions = "After meals"
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var prescription = Assert.IsType<PrescriptionDto>(created.Value);
        Assert.Equal(patient.Id, prescription.PatientId);
        Assert.Equal(
            prescription.IssuedAtUtc.AddDays(PrescriptionPolicies.DefaultExpiryDays),
            prescription.ExpiresAtUtc);
    }

    [Fact]
    public async Task Cancel_endpoint_cancels_active_prescription_with_reason()
    {
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(
            new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == doctorRegistration.DoctorId);
        _host.CurrentUser.UserId = doctor.UserId;

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Patient Cancel",
                null,
                $"patient-cancel-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        var controller = new PrescriptionsController(_host.Sender);

        var created = await controller.CreateAsync(
            new CreatePrescriptionRequest
            {
                PatientId = patient.Id,
                MedicationName = "Ibuprofen",
                Dosage = "400mg",
                Frequency = "Daily",
                DurationDays = 3
            },
            CancellationToken.None);

        var prescription = Assert.IsType<PrescriptionDto>(
            Assert.IsType<CreatedResult>(created.Result).Value);

        var cancelled = await controller.CancelAsync(
            prescription.Id,
            new CancelPrescriptionRequest { Reason = "Duplicate prescription issued" },
            CancellationToken.None);

        var dto = Assert.IsType<PrescriptionDto>(Assert.IsType<OkObjectResult>(cancelled.Result).Value);
        Assert.Equal("cancelled", dto.Status);
        Assert.Equal("Duplicate prescription issued", dto.CancellationReason);
    }
}
