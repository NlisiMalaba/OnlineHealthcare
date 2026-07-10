using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Application.MentalHealth.MoodLogs.GrantMoodChartSharingConsent;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class MoodChartDataAggregationTests : IAsyncLifetime
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc);

    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost(timeProvider: new FakeTimeProvider(ReferenceNowUtc));
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Chart_aggregates_logs_in_ascending_logged_at_order()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var controller = new MoodLogsController(_host.Sender);

        await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 4, LoggedAtUtc = ReferenceNowUtc.AddDays(-1) },
            CancellationToken.None);
        await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 2, LoggedAtUtc = ReferenceNowUtc.AddDays(-3) },
            CancellationToken.None);
        await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 5, LoggedAtUtc = ReferenceNowUtc },
            CancellationToken.None);

        var chart = await controller.GetChartAsync(null, null, CancellationToken.None);
        var payload = Assert.IsType<OkObjectResult>(chart.Result);
        var data = Assert.IsType<MoodChartDataDto>(payload.Value);

        Assert.Equal(3, data.DataPoints.Count);
        Assert.Equal(2, data.DataPoints[0].Rating);
        Assert.Equal(4, data.DataPoints[1].Rating);
        Assert.Equal(5, data.DataPoints[2].Rating);
        Assert.True(data.DataPoints.SequenceEqual(data.DataPoints.OrderBy(point => point.LoggedAtUtc)));
    }

    [Fact]
    public async Task Chart_applies_default_ninety_day_window()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var controller = new MoodLogsController(_host.Sender);

        await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 1, LoggedAtUtc = ReferenceNowUtc.AddDays(-100) },
            CancellationToken.None);
        await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 3, LoggedAtUtc = ReferenceNowUtc.AddDays(-30) },
            CancellationToken.None);

        var chart = await controller.GetChartAsync(null, null, CancellationToken.None);
        var payload = Assert.IsType<OkObjectResult>(chart.Result);
        var data = Assert.IsType<MoodChartDataDto>(payload.Value);

        Assert.Equal(ReferenceNowUtc.AddDays(-90), data.FromUtc);
        Assert.Equal(ReferenceNowUtc, data.ToUtc);
        Assert.Single(data.DataPoints);
        Assert.Equal(3, data.DataPoints[0].Rating);
    }

    [Fact]
    public async Task Chart_filters_logs_outside_requested_range()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var controller = new MoodLogsController(_host.Sender);

        await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 1, LoggedAtUtc = ReferenceNowUtc.AddDays(-10) },
            CancellationToken.None);
        await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 5, LoggedAtUtc = ReferenceNowUtc.AddDays(-2) },
            CancellationToken.None);

        var fromUtc = ReferenceNowUtc.AddDays(-5);
        var toUtc = ReferenceNowUtc.AddDays(-1);
        var chart = await controller.GetChartAsync(fromUtc, toUtc, CancellationToken.None);
        var payload = Assert.IsType<OkObjectResult>(chart.Result);
        var data = Assert.IsType<MoodChartDataDto>(payload.Value);

        Assert.Equal(fromUtc, data.FromUtc);
        Assert.Equal(toUtc, data.ToUtc);
        Assert.Single(data.DataPoints);
        Assert.Equal(5, data.DataPoints[0].Rating);
    }

    [Fact]
    public async Task Therapist_chart_aggregates_patient_logs_when_consent_granted()
    {
        var patient = await SeedPatientAsync();
        var therapist = await SeedTherapistAsync();

        _host.CurrentUser.UserId = patient.UserId;
        var patientController = new MoodLogsController(_host.Sender);
        await patientController.CreateAsync(
            new CreateMoodLogRequest { Rating = 2, LoggedAtUtc = ReferenceNowUtc.AddDays(-1) },
            CancellationToken.None);
        await patientController.CreateAsync(
            new CreateMoodLogRequest { Rating = 4, LoggedAtUtc = ReferenceNowUtc },
            CancellationToken.None);
        await _host.Sender.Send(new GrantMoodChartSharingConsentCommand(therapist.Id), CancellationToken.None);

        _host.CurrentUser.UserId = therapist.UserId;
        var therapistController = new PatientMoodLogsController(_host.Sender);
        var chart = await therapistController.GetChartAsync(patient.Id, null, null, CancellationToken.None);
        var payload = Assert.IsType<OkObjectResult>(chart.Result);
        var data = Assert.IsType<MoodChartDataDto>(payload.Value);

        Assert.Equal(patient.Id, data.PatientId);
        Assert.Equal(2, data.DataPoints.Count);
        Assert.Equal(2, data.DataPoints[0].Rating);
        Assert.Equal(4, data.DataPoints[1].Rating);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Mood Chart Patient",
                null,
                $"mood-chart-{Guid.NewGuid():N}@example.com",
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
