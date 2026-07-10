using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;
using HealthPlatform.Application.Maternal.BirthPlans;
using HealthPlatform.Application.Maternal.BirthPlans.CreateBirthPlan;
using HealthPlatform.Application.Maternal.BirthPlans.GetBirthPlan;
using HealthPlatform.Application.Maternal.BirthPlans.GrantMaternalCareAccess;
using HealthPlatform.Application.Maternal.BirthPlans.UpdateBirthPlan;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class BirthPlanWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Update_birth_plan_notifies_assigned_obstetric_doctor()
    {
        var (recordId, obstetricDoctor, patient) = await SeedAntenatalRecordAsync();
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(
            new CreateBirthPlanCommand(
                recordId,
                new BirthPlanContentDto("Mobility during labour", null, null, null)),
            CancellationToken.None);

        _host.BirthPlanUpdatedNotifier.Calls.Clear();

        await _host.Sender.Send(
            new UpdateBirthPlanCommand(
                recordId,
                new BirthPlanContentDto(
                    "Mobility during labour",
                    "Vaginal delivery preferred",
                    "Gas and air",
                    "Skin-to-skin immediately")),
            CancellationToken.None);

        Assert.Single(_host.BirthPlanUpdatedNotifier.Calls);
        Assert.Equal(obstetricDoctor.UserId, _host.BirthPlanUpdatedNotifier.Calls[0].ObstetricDoctorUserId);
    }

    [Fact]
    public async Task Shared_doctor_can_read_birth_plan()
    {
        var (recordId, obstetricDoctor, patient) = await SeedAntenatalRecordAsync();
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(
            new CreateBirthPlanCommand(
                recordId,
                new BirthPlanContentDto("Partner present", null, null, null)),
            CancellationToken.None);

        var sharedDoctor = await SeedVerifiedDoctorAsync("shared");
        await _host.Sender.Send(
            new GrantMaternalCareAccessCommand(recordId, sharedDoctor.Id, false, true),
            CancellationToken.None);

        _host.CurrentUser.UserId = sharedDoctor.UserId;
        var birthPlan = await _host.Sender.Send(new GetBirthPlanQuery(recordId), CancellationToken.None);

        Assert.Equal("Partner present", birthPlan.Content.LabourPreferences);

        _host.CurrentUser.UserId = obstetricDoctor.UserId;
        var obstetricView = await _host.Sender.Send(new GetBirthPlanQuery(recordId), CancellationToken.None);
        Assert.Equal(birthPlan.Id, obstetricView.Id);
    }

    [Fact]
    public async Task Unshared_doctor_cannot_read_birth_plan()
    {
        var (recordId, _, patient) = await SeedAntenatalRecordAsync();
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(
            new CreateBirthPlanCommand(
                recordId,
                new BirthPlanContentDto("Quiet room", null, null, null)),
            CancellationToken.None);

        var outsider = await SeedVerifiedDoctorAsync("outsider");
        _host.CurrentUser.UserId = outsider.UserId;

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(() =>
            _host.Sender.Send(new GetBirthPlanQuery(recordId), CancellationToken.None));

        Assert.Equal(BirthPlanErrorCodes.AccessDenied, ex.Code);
    }

    private async Task<(Guid RecordId, Doctor ObstetricDoctor, Patient Patient)> SeedAntenatalRecordAsync()
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

        return (record.Id, obstetricDoctor, patient);
    }

    private async Task<Doctor> SeedVerifiedObstetricDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            ObstetricDoctorRegistrationTestData.CreateValidCommand(
                $"birth-plan-obstetric-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Doctor> SeedVerifiedDoctorAsync(string suffix)
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(
                $"birth-plan-{suffix}-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Birth Plan Patient",
                null,
                $"birth-plan-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
