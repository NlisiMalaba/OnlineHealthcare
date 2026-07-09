using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Queue.Manage;
using HealthPlatform.Application.Queue.JoinQueue;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Queue;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Queue;

public sealed class QueueManagementCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Advance_on_zero_length_queue_returns_empty_result()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        _host.CurrentUser.UserId = doctor.UserId;

        var queue = await _host.Sender.Send(new AdvanceQueueCommand(), CancellationToken.None);

        Assert.Empty(queue);
    }

    [Fact]
    public async Task Advance_marks_head_as_called_and_keeps_positions_consistent()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var firstEntry = await JoinQueueAsNewPatientAsync(doctor, "first");
        var secondEntry = await JoinQueueAsNewPatientAsync(doctor, "second");

        _host.CurrentUser.UserId = doctor.UserId;
        var queue = await _host.Sender.Send(new AdvanceQueueCommand(), CancellationToken.None);

        Assert.Equal(2, queue.Count);
        Assert.Equal(firstEntry.Id, queue[0].Id);
        Assert.Equal("called", queue[0].ArrivalStatus);
        Assert.Equal(1, queue[0].QueuePosition);
        Assert.Equal(secondEntry.Id, queue[1].Id);
        Assert.Equal(2, queue[1].QueuePosition);
    }

    [Fact]
    public async Task Mark_seen_records_actual_wait_and_recalculates_remaining_entries()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var firstEntry = await JoinQueueAsNewPatientAsync(doctor, "first");
        var secondEntry = await JoinQueueAsNewPatientAsync(doctor, "second");

        _host.CurrentUser.UserId = doctor.UserId;
        var seen = await _host.Sender.Send(new MarkQueueEntrySeenCommand(firstEntry.Id), CancellationToken.None);

        Assert.Equal("seen", seen.ArrivalStatus);
        var storedSeen = await _host.DbContext.QueueEntries.SingleAsync(entry => entry.Id == firstEntry.Id);
        Assert.True(storedSeen.ActualWaitMinutes.HasValue);
        Assert.True(storedSeen.ActualWaitMinutes.Value >= 0);

        var updatedSecond = await _host.DbContext.QueueEntries.SingleAsync(entry => entry.Id == secondEntry.Id);
        Assert.Equal(1, updatedSecond.QueuePosition);
        Assert.Equal(0, updatedSecond.EstimatedWaitMinutes);
    }

    [Fact]
    public async Task Mark_absent_removes_entry_and_notifies_patient()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var firstEntry = await JoinQueueAsNewPatientAsync(doctor, "first");
        var secondEntry = await JoinQueueAsNewPatientAsync(doctor, "second");
        var firstPatient = await _host.DbContext.Patients.SingleAsync(p => p.Id == firstEntry.PatientId);

        _host.CurrentUser.UserId = doctor.UserId;
        await _host.Sender.Send(new MarkQueueEntryAbsentCommand(firstEntry.Id), CancellationToken.None);

        Assert.False(await _host.DbContext.QueueEntries.AnyAsync(entry => entry.Id == firstEntry.Id));
        var remaining = await _host.DbContext.QueueEntries.SingleAsync(entry => entry.Id == secondEntry.Id);
        Assert.Equal(1, remaining.QueuePosition);
        Assert.Equal(0, remaining.EstimatedWaitMinutes);

        Assert.Single(_host.QueueStatusNotifier.Calls);
        var notification = _host.QueueStatusNotifier.Calls[0];
        Assert.Equal(firstPatient.UserId, notification.PatientUserId);
        Assert.Equal(firstEntry.Id, notification.QueueEntryId);
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

    private async Task<QueueEntry> JoinQueueAsNewPatientAsync(Doctor doctor, string suffix)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Queue Manage Patient {suffix}",
                null,
                $"queue-manage-{suffix}-{Guid.NewGuid():N}@example.com",
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
        var joined = await _host.Sender.Send(new JoinQueueCommand(appointment.Id), CancellationToken.None);
        return await _host.DbContext.QueueEntries.SingleAsync(entry => entry.Id == joined.Id);
    }
}
