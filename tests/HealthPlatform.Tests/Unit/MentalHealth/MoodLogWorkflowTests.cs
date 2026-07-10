using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Application.MentalHealth.MoodLogs.GrantMoodChartSharingConsent;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.MongoDb;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class MoodLogWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Patient_can_create_update_list_and_delete_mood_logs()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var controller = new MoodLogsController(_host.Sender);

        var created = await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 3, Notes = "Steady day" },
            CancellationToken.None);
        var createdResult = Assert.IsType<CreatedResult>(created.Result);
        var moodLog = Assert.IsType<MoodLogDto>(createdResult.Value);

        var updated = await controller.UpdateAsync(
            moodLog.Id,
            new UpdateMoodLogRequest { Rating = 4, Notes = "Improved" },
            CancellationToken.None);
        var updatedPayload = Assert.IsType<OkObjectResult>(updated.Result);
        Assert.Equal(4, Assert.IsType<MoodLogDto>(updatedPayload.Value).Rating);

        var list = await controller.ListAsync(null, null, CancellationToken.None);
        var listPayload = Assert.IsType<OkObjectResult>(list.Result);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<MoodLogDto>>(listPayload.Value));

        Assert.IsType<NoContentResult>(await controller.DeleteAsync(moodLog.Id, CancellationToken.None));

        var afterDelete = await controller.ListAsync(null, null, CancellationToken.None);
        var afterDeletePayload = Assert.IsType<OkObjectResult>(afterDelete.Result);
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<MoodLogDto>>(afterDeletePayload.Value));
    }

    [Fact]
    public async Task Patient_chart_returns_time_series_points()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var controller = new MoodLogsController(_host.Sender);

        await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 2, LoggedAtUtc = DateTime.UtcNow.AddDays(-2) },
            CancellationToken.None);
        await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 5, LoggedAtUtc = DateTime.UtcNow.AddDays(-1) },
            CancellationToken.None);

        var chart = await controller.GetChartAsync(null, null, CancellationToken.None);
        var chartPayload = Assert.IsType<OkObjectResult>(chart.Result);
        var data = Assert.IsType<MoodChartDataDto>(chartPayload.Value);
        Assert.Equal(patient.Id, data.PatientId);
        Assert.Equal(2, data.DataPoints.Count);
        Assert.Equal(2, data.DataPoints[0].Rating);
        Assert.Equal(5, data.DataPoints[1].Rating);
    }

    [Fact]
    public async Task Therapist_requires_consent_to_view_patient_chart()
    {
        var patient = await SeedPatientAsync();
        var therapist = await SeedTherapistAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var patientController = new MoodLogsController(_host.Sender);
        await patientController.CreateAsync(
            new CreateMoodLogRequest { Rating = 1 },
            CancellationToken.None);

        _host.CurrentUser.UserId = therapist.UserId;
        var therapistController = new PatientMoodLogsController(_host.Sender);

        var denied = await Assert.ThrowsAsync<AccessDeniedException>(() =>
            therapistController.GetChartAsync(patient.Id, null, null, CancellationToken.None));
        Assert.Equal(MoodLogErrorCodes.MoodChartConsentRequired, denied.Code);

        _host.CurrentUser.UserId = patient.UserId;
        await _host.Sender.Send(new GrantMoodChartSharingConsentCommand(therapist.Id), CancellationToken.None);

        _host.CurrentUser.UserId = therapist.UserId;
        var chart = await therapistController.GetChartAsync(patient.Id, null, null, CancellationToken.None);
        var chartPayload = Assert.IsType<OkObjectResult>(chart.Result);
        Assert.Single(Assert.IsType<MoodChartDataDto>(chartPayload.Value).DataPoints);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Mood Patient",
                null,
                $"mood-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }

    private async Task<Doctor> SeedTherapistAsync()
    {
        var registration = await _host.Sender.Send(
            TherapistRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }
}
