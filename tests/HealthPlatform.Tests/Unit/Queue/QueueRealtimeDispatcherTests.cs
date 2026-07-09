using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Queue;
using HealthPlatform.Application.Queue.JoinQueue;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Queue;

public sealed class QueueRealtimeDispatcherTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Dispatch_publishes_realtime_updates_and_notifies_only_once_at_position_two()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var firstPatient = await SeedPatientAsync("first");
        var secondPatient = await SeedPatientAsync("second");

        var firstAppointment = await SeedConfirmedAppointmentAsync(doctor, firstPatient);
        _host.CurrentUser.UserId = firstPatient.UserId;
        await _host.Sender.Send(new JoinQueueCommand(firstAppointment.Id), CancellationToken.None);

        var secondAppointment = await SeedConfirmedAppointmentAsync(doctor, secondPatient);
        _host.CurrentUser.UserId = secondPatient.UserId;
        var secondQueueEntry = await _host.Sender.Send(new JoinQueueCommand(secondAppointment.Id), CancellationToken.None);

        var dispatcher = _host.GetRequiredService<IQueueRealtimeDispatcher>();

        var firstTickCount = await dispatcher.DispatchAsync(CancellationToken.None);
        var secondTickCount = await dispatcher.DispatchAsync(CancellationToken.None);

        Assert.Equal(2, firstTickCount);
        Assert.Equal(2, secondTickCount);
        Assert.Equal(4, _host.QueueRealtimeNotifier.Updates.Count);

        Assert.Single(_host.QueuePositionNotifier.Calls);
        var notification = _host.QueuePositionNotifier.Calls[0];
        Assert.Equal(secondPatient.UserId, notification.PatientUserId);
        Assert.Equal(secondQueueEntry.Id, notification.QueueEntryId);
        Assert.Equal(secondAppointment.Id, notification.AppointmentId);
        Assert.Equal(secondQueueEntry.EstimatedWaitMinutes, notification.EstimatedWaitMinutes);

        var storedEntry = await _host.DbContext.QueueEntries.SingleAsync(entry => entry.Id == secondQueueEntry.Id);
        Assert.NotNull(storedEntry.PositionTwoNotifiedAtUtc);
    }

    private async Task<Doctor> SeedVerifiedDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync(string suffix)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Queue Realtime Patient {suffix}",
                null,
                $"queue-realtime-{suffix}-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }

    private async Task<Appointment> SeedConfirmedAppointmentAsync(Doctor doctor, Patient patient)
    {
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
        return appointment;
    }
}
