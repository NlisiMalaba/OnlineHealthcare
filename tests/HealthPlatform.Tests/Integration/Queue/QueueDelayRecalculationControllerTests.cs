using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Queue;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Queue;

public sealed class QueueDelayRecalculationControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Recalculate_delay_endpoint_updates_queue_estimates()
    {
        var registration = await _host.Sender.Send(DoctorRegistrationTestData.CreateValidCommand(), CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        var doctor = await _host.DbContext.Doctors.Include(d => d.AvailabilitySlots).SingleAsync(d => d.Id == registration.DoctorId);

        await JoinQueueAsPatientAsync(doctor, "first");
        await JoinQueueAsPatientAsync(doctor, "second");

        _host.CurrentUser.UserId = doctor.UserId;
        var controller = new QueueController(_host.Sender);
        var response = await controller.RecalculateOnDelayAsync(
            new RecalculateQueueOnDelayRequest { DelayMinutes = 20 },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<List<HealthPlatform.Application.Queue.QueueEntryDto>>(ok.Value);
        Assert.Equal(2, payload.Count);
        Assert.Equal(20, payload[0].EstimatedWaitMinutes);
        Assert.Equal(50, payload[1].EstimatedWaitMinutes);
    }

    private async Task JoinQueueAsPatientAsync(Doctor doctor, string suffix)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Queue Delay Integration {suffix}",
                null,
                $"queue-delay-int-{suffix}-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        var slot = doctor.AvailabilitySlots.Single();
        var appointment = Appointment.CreatePendingPayment(
            patient.Id,
            doctor.Id,
            slot.Id,
            ConsultationType.General,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddMinutes(10));
        appointment.ConfirmOnPayment(DateTime.UtcNow);
        _host.DbContext.Appointments.Add(appointment);
        await _host.DbContext.SaveChangesAsync();

        _host.CurrentUser.UserId = patient.UserId;
        _ = await new QueueController(_host.Sender).JoinAsync(
            new JoinQueueRequest { AppointmentId = appointment.Id },
            CancellationToken.None);
    }
}
