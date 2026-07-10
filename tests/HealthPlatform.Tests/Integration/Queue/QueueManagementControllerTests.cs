using HealthPlatform.API.Controllers;
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

public sealed class QueueManagementControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Management_endpoints_advance_seen_and_absent_update_queue_state()
    {
        var registration = await _host.Sender.Send(DoctorRegistrationTestData.CreateValidCommand(), CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        var doctor = await _host.DbContext.Doctors.Include(d => d.AvailabilitySlots).SingleAsync(d => d.Id == registration.DoctorId);

        var firstEntryId = await JoinQueueAsPatientAsync(doctor, "first");
        var secondEntryId = await JoinQueueAsPatientAsync(doctor, "second");

        _host.CurrentUser.UserId = doctor.UserId;
        var controller = new QueueController(_host.Sender);

        var advance = await controller.AdvanceAsync(CancellationToken.None);
        var advanced = Assert.IsType<OkObjectResult>(advance.Result).Value as IReadOnlyList<HealthPlatform.Application.Queue.QueueEntryDto>;
        Assert.NotNull(advanced);
        Assert.Equal("called", advanced![0].ArrivalStatus);

        var seen = await controller.MarkSeenAsync(firstEntryId, CancellationToken.None);
        var seenPayload = Assert.IsType<OkObjectResult>(seen.Result).Value as HealthPlatform.Application.Queue.QueueEntryDto;
        Assert.NotNull(seenPayload);
        Assert.Equal("seen", seenPayload!.ArrivalStatus);

        var absentResult = await controller.MarkAbsentAsync(secondEntryId, CancellationToken.None);
        Assert.IsType<NoContentResult>(absentResult);
    }

    private async Task<Guid> JoinQueueAsPatientAsync(Doctor doctor, string suffix)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Queue Integration Patient {suffix}",
                null,
                $"queue-mgmt-int-{suffix}-{Guid.NewGuid():N}@example.com",
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
        var joinResult = await new QueueController(_host.Sender).JoinAsync(
            new HealthPlatform.API.Requests.Queue.JoinQueueRequest { AppointmentId = appointment.Id },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(joinResult.Result);
        var payload = Assert.IsType<HealthPlatform.Application.Queue.QueueEntryDto>(created.Value);
        return payload.Id;
    }
}
