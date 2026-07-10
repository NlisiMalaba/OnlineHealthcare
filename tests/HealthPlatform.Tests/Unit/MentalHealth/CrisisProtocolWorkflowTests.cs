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

public sealed class CrisisProtocolWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Mood_log_with_crisis_keyword_returns_helplines_and_notifies_mental_health_contact()
    {
        var patient = await SeedPatientWithMentalHealthContactAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var controller = new MoodLogsController(_host.Sender);
        var result = await controller.CreateAsync(
            new CreateMoodLogRequest { Rating = 1, Notes = "I feel suicidal tonight" },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var payload = Assert.IsType<Application.MentalHealth.MoodLogs.MoodLogMutationResultDto>(created.Value);
        Assert.True(payload.CrisisProtocol.Triggered);
        Assert.NotEmpty(payload.CrisisProtocol.Helplines);
        Assert.Contains("emergency services", payload.CrisisProtocol.EmergencyServicesPrompt, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, payload.CrisisProtocol.MentalHealthContactsNotified);

        Assert.Single(_host.NextOfKinEmergencyAlertNotifier.Calls);
        Assert.Single(_host.NextOfKinEmergencyAlertNotifier.Calls[0].ContactIds);

        var alert = await _host.DbContext.EmergencyAlerts.SingleAsync();
        Assert.Equal(EmergencyAlertTriggerSource.MentalHealthCrisis, alert.TriggerSource);
    }

    [Fact]
    public async Task Ai_assistant_evaluate_endpoint_triggers_crisis_protocol()
    {
        var patient = await SeedPatientWithMentalHealthContactAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var controller = new CrisisProtocolController(_host.Sender);
        var result = await controller.EvaluateAsync(
            new EvaluateCrisisInputRequest { InputText = "I want to end my life" },
            CancellationToken.None);

        var payload = Assert.IsType<OkObjectResult>(result.Result);
        var crisisProtocol = Assert.IsType<Application.MentalHealth.CrisisProtocol.CrisisProtocolDto>(payload.Value);
        Assert.True(crisisProtocol.Triggered);
        Assert.NotEmpty(crisisProtocol.Helplines);
        Assert.Single(_host.NextOfKinEmergencyAlertNotifier.Calls);
    }

    [Fact]
    public async Task Crisis_protocol_without_mental_health_contact_still_returns_helplines()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var controller = new CrisisProtocolController(_host.Sender);
        var result = await controller.EvaluateAsync(
            new EvaluateCrisisInputRequest { InputText = "I feel suicidal" },
            CancellationToken.None);

        var payload = Assert.IsType<OkObjectResult>(result.Result);
        var crisisProtocol = Assert.IsType<Application.MentalHealth.CrisisProtocol.CrisisProtocolDto>(payload.Value);
        Assert.True(crisisProtocol.Triggered);
        Assert.Equal(0, crisisProtocol.MentalHealthContactsNotified);
        Assert.Empty(_host.NextOfKinEmergencyAlertNotifier.Calls);

        var alert = await _host.DbContext.EmergencyAlerts.SingleAsync();
        Assert.Equal(EmergencyAlertOverallStatus.NoContacts, alert.OverallStatus);
    }

    [Fact]
    public async Task Non_crisis_input_does_not_trigger_protocol()
    {
        var patient = await SeedPatientWithMentalHealthContactAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var controller = new CrisisProtocolController(_host.Sender);
        var result = await controller.EvaluateAsync(
            new EvaluateCrisisInputRequest { InputText = "I had a stressful day" },
            CancellationToken.None);

        var payload = Assert.IsType<OkObjectResult>(result.Result);
        var crisisProtocol = Assert.IsType<Application.MentalHealth.CrisisProtocol.CrisisProtocolDto>(payload.Value);
        Assert.False(crisisProtocol.Triggered);
        Assert.Empty(_host.NextOfKinEmergencyAlertNotifier.Calls);
        Assert.False(await _host.DbContext.EmergencyAlerts.AnyAsync());
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Crisis Protocol Patient",
                null,
                $"crisis-protocol-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }

    private async Task<Patient> SeedPatientWithMentalHealthContactAsync()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(
            new AddNextOfKinContactCommand(
                "Mental Health Contact",
                "Sibling",
                "+15550009999",
                "mh-contact@example.com",
                true),
            CancellationToken.None);

        await _host.Sender.Send(
            new AddNextOfKinContactCommand(
                "General Contact",
                "Parent",
                "+15550008888",
                "general-contact@example.com",
                false),
            CancellationToken.None);

        return patient;
    }
}
