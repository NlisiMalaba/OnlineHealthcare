using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.NextOfKin.AddNextOfKinContact;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class CrisisProtocolNoMentalHealthContactTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Crisis_protocol_with_only_general_next_of_kin_returns_helplines_without_notifications()
    {
        var patient = await SeedPatientWithGeneralContactOnlyAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var controller = new MoodLogsController(_host.Sender);
        var result = await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 2, Notes = "I feel suicidal and alone" },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var payload = Assert.IsType<Application.MentalHealth.MoodLogs.MoodLogMutationResultDto>(created.Value);

        Assert.True(payload.CrisisProtocol.Triggered);
        Assert.NotEmpty(payload.CrisisProtocol.Helplines);
        Assert.Equal(0, payload.CrisisProtocol.MentalHealthContactsNotified);
        Assert.Empty(_host.NextOfKinEmergencyAlertNotifier.Calls);

        var alert = await _host.DbContext.EmergencyAlerts.SingleAsync();
        Assert.Equal(EmergencyAlertTriggerSource.MentalHealthCrisis, alert.TriggerSource);
        Assert.Equal(EmergencyAlertOverallStatus.NoContacts, alert.OverallStatus);
    }

    [Fact]
    public async Task Crisis_protocol_evaluate_with_only_general_next_of_kin_does_not_notify_contacts()
    {
        var patient = await SeedPatientWithGeneralContactOnlyAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var controller = new CrisisProtocolController(_host.Sender);
        var result = await controller.EvaluateAsync(
            new EvaluateCrisisInputRequest { InputText = "I want to hurt myself" },
            CancellationToken.None);

        var payload = Assert.IsType<OkObjectResult>(result.Result);
        var crisisProtocol = Assert.IsType<Application.MentalHealth.CrisisProtocol.CrisisProtocolDto>(payload.Value);

        Assert.True(crisisProtocol.Triggered);
        Assert.Equal(0, crisisProtocol.MentalHealthContactsNotified);
        Assert.Empty(_host.NextOfKinEmergencyAlertNotifier.Calls);
    }

    private async Task<Patient> SeedPatientWithGeneralContactOnlyAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Crisis No MH Contact Patient",
                null,
                $"crisis-no-mh-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(
            new AddNextOfKinContactCommand(
                "General Contact",
                "Parent",
                "+15550001111",
                "general-only@example.com",
                false),
            CancellationToken.None);

        return patient;
    }
}
