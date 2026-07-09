using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Queue;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Queue;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Queue;

public sealed class QueueControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Join_endpoint_returns_created_queue_entry()
    {
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(
            new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Queue Patient",
                null,
                $"queue-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        var slot = doctor.AvailabilitySlots.Single();
        var appointment = Appointment.CreatePendingPayment(
            patient.Id,
            doctor.Id,
            slot.Id,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddMinutes(10));
        appointment.ConfirmOnPayment(DateTime.UtcNow);
        _host.DbContext.Appointments.Add(appointment);
        await _host.DbContext.SaveChangesAsync();

        _host.CurrentUser.UserId = patient.UserId;

        var controller = new QueueController(_host.Sender);
        var result = await controller.JoinAsync(
            new JoinQueueRequest { AppointmentId = appointment.Id },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var payload = Assert.IsType<QueueEntryDto>(created.Value);
        Assert.Equal(1, payload.QueuePosition);
        Assert.Equal(0, payload.EstimatedWaitMinutes);
        Assert.Equal(patient.FullName, payload.PatientName);
    }
}
