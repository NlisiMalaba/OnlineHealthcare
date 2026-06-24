using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Appointments;

public sealed class AppointmentsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Book_endpoint_returns_created_pending_payment_appointment()
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
                "Booking Patient",
                null,
                $"booking-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var controller = new AppointmentsController(_host.Sender);
        var result = await controller.BookAsync(
            new BookAppointmentRequest
            {
                DoctorId = doctor.Id,
                SlotId = doctor.AvailabilitySlots.Single().Id,
                ScheduledAtUtc = DateTime.UtcNow.AddDays(1)
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var payload = Assert.IsType<BookAppointmentDto>(created.Value);
        Assert.Equal("pending_payment", payload.Status);
        Assert.NotNull(payload.Clinic);
        Assert.Equal(doctor.ClinicAddress, payload.Clinic.Address);
        Assert.NotNull(payload.Clinic.GpsNavigationUrl);
    }
}
