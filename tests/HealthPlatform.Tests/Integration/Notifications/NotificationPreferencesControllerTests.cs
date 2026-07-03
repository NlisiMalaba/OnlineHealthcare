using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Notifications;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Notifications.UpdateNotificationPreferences;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.Notifications;

public sealed class NotificationPreferencesControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task UpdateAsync_persists_preferences_and_get_returns_updated_channels()
    {
        var patientUserId = await RegisterPatientAsync();
        _host.CurrentUser.UserId = patientUserId;

        var controller = new NotificationPreferencesController(_host.Sender);
        var updateResult = await controller.UpdateAsync(
            new UpdateNotificationPreferencesRequest
            {
                Preferences =
                [
                    new NotificationEventPreferenceUpdateRequest
                    {
                        EventType = NotificationEventTypes.AppointmentConfirmed,
                        Channels =
                        [
                            new NotificationChannelPreferenceUpdateRequest
                            {
                                Channel = "push",
                                IsEnabled = true
                            },
                            new NotificationChannelPreferenceUpdateRequest
                            {
                                Channel = "sms",
                                IsEnabled = false
                            },
                            new NotificationChannelPreferenceUpdateRequest
                            {
                                Channel = "email",
                                IsEnabled = true
                            }
                        ]
                    }
                ]
            },
            CancellationToken.None);

        var updated = Assert.IsType<OkObjectResult>(updateResult.Result);
        var updatedDto = Assert.IsType<NotificationPreferencesDto>(updated.Value);
        var appointmentPreference = Assert.Single(
            updatedDto.Preferences,
            preference => preference.EventType == NotificationEventTypes.AppointmentConfirmed);
        Assert.True(appointmentPreference.Channels.Push);
        Assert.False(appointmentPreference.Channels.Sms);
        Assert.True(appointmentPreference.Channels.Email);

        var getResult = await controller.GetAsync(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(getResult.Result);
        var preferences = Assert.IsType<NotificationPreferencesDto>(ok.Value);
        var persisted = preferences.Preferences.Single(
            preference => preference.EventType == NotificationEventTypes.AppointmentConfirmed);
        Assert.False(persisted.Channels.Sms);
    }

    [Fact]
    public async Task UpdateAsync_rejects_non_configurable_event_type_for_patient()
    {
        var patientUserId = await RegisterPatientAsync();
        _host.CurrentUser.UserId = patientUserId;

        var controller = new NotificationPreferencesController(_host.Sender);
        await Assert.ThrowsAsync<Application.Exceptions.DomainException>(() => controller.UpdateAsync(
            new UpdateNotificationPreferencesRequest
            {
                Preferences =
                [
                    new NotificationEventPreferenceUpdateRequest
                    {
                        EventType = NotificationEventTypes.LowStockAlert,
                        Channels =
                        [
                            new NotificationChannelPreferenceUpdateRequest
                            {
                                Channel = "push",
                                IsEnabled = false
                            }
                        ]
                    }
                ]
            },
            CancellationToken.None));
    }

    private async Task<Guid> RegisterPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Notification Preferences Patient",
                null,
                $"notification-prefs-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).First();
        return patient.UserId;
    }
}
