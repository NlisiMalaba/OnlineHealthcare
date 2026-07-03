using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.NextOfKin.AddNextOfKinContact;
using HealthPlatform.Application.NextOfKin.DispatchSystemEmergencyAlert;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.NextOfKin;

public sealed class EmergencyAlertDispatchServiceTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task DispatchAsync_notifies_all_next_of_kin_contacts_and_logs_alert()
    {
        var patient = await RegisterPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        foreach (var index in new[] { 0, 1 })
        {
            await _host.Sender.Send(
                new AddNextOfKinContactCommand(
                    $"Contact {index}",
                    "Sibling",
                    $"+26377123456{index}",
                    null,
                    false),
                CancellationToken.None);
        }

        var alert = await _host.Sender.Send(
            new DispatchSystemEmergencyAlertCommand(patient.Id, "Patient unresponsive during triage."),
            CancellationToken.None);

        Assert.Equal(EmergencyAlertOverallStatus.Dispatched, alert.OverallStatus);
        Assert.Equal(2, alert.ContactDeliveries.Count);
        Assert.Single(_host.NextOfKinEmergencyAlertNotifier.Calls);
        Assert.Equal(2, _host.NextOfKinEmergencyAlertNotifier.Calls[0].ContactIds.Count);

        var persisted = await _host.DbContext.EmergencyAlerts
            .AsNoTracking()
            .SingleAsync(a => a.Id == alert.Id);
        Assert.Equal("Patient unresponsive during triage.", persisted.TriggerReason);
        Assert.Equal(EmergencyAlertTriggerSource.System, persisted.TriggerSource);
    }

    [Fact]
    public async Task DispatchAsync_logs_alert_when_patient_has_no_contacts()
    {
        var patient = await RegisterPatientAsync();

        var alert = await _host.Sender.Send(
            new DispatchSystemEmergencyAlertCommand(patient.Id, "Critical biometric reading."),
            CancellationToken.None);

        Assert.Equal(EmergencyAlertOverallStatus.NoContacts, alert.OverallStatus);
        Assert.Empty(alert.ContactDeliveries);
        Assert.Empty(_host.NextOfKinEmergencyAlertNotifier.Calls);
    }

    private async Task<Domain.Identity.Patient> RegisterPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Emergency Alert Patient",
                null,
                $"emergency-alert-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }
}
