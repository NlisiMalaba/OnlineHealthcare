using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Maternal;

public sealed class BirthPlansControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_and_update_birth_plan_through_patient_controller()
    {
        var obstetricDoctor = await SeedVerifiedObstetricDoctorAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var record = await _host.Sender.Send(
            new CreateAntenatalRecordCommand(
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(120)),
                12,
                obstetricDoctor.Id),
            CancellationToken.None);

        var controller = new AntenatalRecordsController(_host.Sender);
        var created = await controller.CreateBirthPlanAsync(
            record.Id,
            new BirthPlanContentRequest("Water birth preferred", null, null, null),
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(created.Result);
        var birthPlan = Assert.IsType<HealthPlatform.Application.Maternal.BirthPlans.BirthPlanDto>(createdResult.Value);
        Assert.Equal("Water birth preferred", birthPlan.Content.LabourPreferences);

        var updated = await controller.UpdateBirthPlanAsync(
            record.Id,
            new BirthPlanContentRequest(
                "Water birth preferred",
                "Vaginal",
                "Hypnobirthing",
                "Delayed cord clamping"),
            CancellationToken.None);

        var updatedPlan = Assert.IsType<HealthPlatform.Application.Maternal.BirthPlans.BirthPlanDto>(
            Assert.IsType<OkObjectResult>(updated.Result).Value);
        Assert.Equal("Hypnobirthing", updatedPlan.Content.PainManagement);
        Assert.Single(_host.BirthPlanUpdatedNotifier.Calls);
    }

    private async Task<Doctor> SeedVerifiedObstetricDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            ObstetricDoctorRegistrationTestData.CreateValidCommand(
                $"birth-plan-controller-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Birth Plan Controller Patient",
                null,
                $"birth-plan-controller-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
