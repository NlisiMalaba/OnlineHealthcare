using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Appointments;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Appointments.RescheduleAppointment;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Appointments;

public sealed class DoctorAppointmentsRescheduleControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Reschedule_endpoint_returns_updated_appointment()
    {
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Controller Reschedule Patient",
                null,
                $"controller-reschedule-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var originalScheduledAt = DateTime.UtcNow.AddHours(6);
        var booking = await _host.Sender.Send(
            new HealthPlatform.Application.Appointments.BookAppointment.BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                originalScheduledAt),
            CancellationToken.None);

        await new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            _host.GetRequiredService<HealthPlatform.Application.Appointments.IAppointmentRepository>(),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>())
            .Handle(
                new PaymentCompletedNotification(
                    booking.AppointmentId,
                    Guid.CreateVersion7(),
                    DateTime.UtcNow),
                CancellationToken.None);

        var newScheduledAt = originalScheduledAt.AddDays(1);
        _host.CurrentUser.UserId = doctor.UserId;

        var controller = new DoctorAppointmentsController(_host.Sender);
        var result = await controller.RescheduleAsync(
            booking.AppointmentId,
            new RescheduleAppointmentRequest
            {
                NewSlotId = doctor.AvailabilitySlots.First().Id,
                NewScheduledAtUtc = newScheduledAt
            },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<RescheduleAppointmentDto>(ok.Value);
        Assert.Equal(newScheduledAt, payload.ScheduledAtUtc);
        Assert.Equal(originalScheduledAt, payload.PreviousScheduledAtUtc);
        Assert.Equal("confirmed", payload.Status);
    }
}
