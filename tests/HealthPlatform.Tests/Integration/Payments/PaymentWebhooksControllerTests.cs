using System.Text;
using HealthPlatform.API.Controllers;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Payments.Webhooks;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Payments;

public sealed class PaymentWebhooksControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Flutterwave_webhook_endpoint_confirms_pending_appointment()
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
                "Webhook Patient",
                null,
                $"webhook-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                DateTime.UtcNow.AddHours(6)),
            CancellationToken.None);

        var controller = new PaymentWebhooksController(_host.Sender)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var rawBody = $$"""
            {
              "event": "charge.completed",
              "data": {
                "id": 4242,
                "status": "successful",
                "amount": 10.00,
                "currency": "USD",
                "flw_ref": "FLW-REF-42",
                "meta": {
                  "appointment_id": "{{booking.AppointmentId}}"
                }
              }
            }
            """;

        controller.ControllerContext.HttpContext.Request.Headers["verif-hash"] = "dev:test";
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(rawBody));

        var actionResult = await controller.FlutterwaveAsync(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var payload = Assert.IsType<ProcessPaymentWebhookResultDto>(ok.Value);

        Assert.True(payload.Accepted);
        Assert.False(payload.Duplicate);
        Assert.Equal(PaymentWebhookEventStatus.Completed, payload.Status);

        var appointment = await _host.DbContext.Appointments.SingleAsync(a => a.Id == booking.AppointmentId);
        Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
    }
}
