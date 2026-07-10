using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Queue.Manage;
using HealthPlatform.Application.Queue.JoinQueue;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Queue;

public sealed class RecalculateQueueOnDelayCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Recalculate_delay_updates_all_estimates_and_notifies_affected_patients()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var first = await JoinQueueAsNewPatientAsync(doctor, "first");
        var second = await JoinQueueAsNewPatientAsync(doctor, "second");

        _host.CurrentUser.UserId = doctor.UserId;
        var updated = await _host.Sender.Send(new RecalculateQueueOnDelayCommand(20), CancellationToken.None);

        Assert.Equal(2, updated.Count);
        Assert.Equal(20, updated[0].EstimatedWaitMinutes);
        Assert.Equal(50, updated[1].EstimatedWaitMinutes);
        Assert.Equal(2, _host.QueueDelayNotifier.Calls.Count);
        Assert.Contains(_host.QueueDelayNotifier.Calls, call => call.QueueEntryId == first.Id && call.DelayMinutes == 20);
        Assert.Contains(_host.QueueDelayNotifier.Calls, call => call.QueueEntryId == second.Id && call.DelayMinutes == 20);
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

    private async Task<HealthPlatform.Domain.Queue.QueueEntry> JoinQueueAsNewPatientAsync(Doctor doctor, string suffix)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Queue Delay Patient {suffix}",
                null,
                $"queue-delay-{suffix}-{Guid.NewGuid():N}@example.com",
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
        var joined = await _host.Sender.Send(new JoinQueueCommand(appointment.Id), CancellationToken.None);
        return await _host.DbContext.QueueEntries.SingleAsync(entry => entry.Id == joined.Id);
    }
}
