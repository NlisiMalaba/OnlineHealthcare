using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Queue.JoinQueue;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Queue;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Queue;

public sealed class JoinQueueCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Join_queue_assigns_position_and_wait_time_for_first_patient()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var appointment = await SeedConfirmedAppointmentAsync(doctor, patient);
        _host.CurrentUser.UserId = patient.UserId;

        var result = await _host.Sender.Send(new JoinQueueCommand(appointment.Id), CancellationToken.None);

        Assert.Equal(1, result.QueuePosition);
        Assert.Equal(0, result.EstimatedWaitMinutes);
        Assert.Equal(patient.FullName, result.PatientName);
        Assert.Equal(appointment.ScheduledAtUtc, result.AppointmentScheduledAtUtc);
        Assert.Equal("not_arrived", result.ArrivalStatus);

        var stored = await _host.DbContext.QueueEntries.SingleAsync(entry => entry.Id == result.Id);
        Assert.Equal(doctor.Id, stored.DoctorId);
    }

    [Fact]
    public async Task Join_queue_computes_wait_time_from_patients_ahead_and_slot_duration()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var firstPatient = await SeedPatientAsync("first");
        var secondPatient = await SeedPatientAsync("second");
        var firstAppointment = await SeedConfirmedAppointmentAsync(doctor, firstPatient);
        var secondAppointment = await SeedConfirmedAppointmentAsync(doctor, secondPatient);

        _host.CurrentUser.UserId = firstPatient.UserId;
        await _host.Sender.Send(new JoinQueueCommand(firstAppointment.Id), CancellationToken.None);

        _host.CurrentUser.UserId = secondPatient.UserId;
        var result = await _host.Sender.Send(new JoinQueueCommand(secondAppointment.Id), CancellationToken.None);

        Assert.Equal(2, result.QueuePosition);
        Assert.Equal(30, result.EstimatedWaitMinutes);
    }

    [Fact]
    public async Task Join_queue_rejects_virtual_only_appointment()
    {
        var doctor = await SeedVerifiedDoctorAsync(DoctorAppointmentType.Virtual);
        var patient = await SeedPatientAsync("virtual");
        var appointment = await SeedConfirmedAppointmentAsync(doctor, patient);
        _host.CurrentUser.UserId = patient.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _host.Sender.Send(new JoinQueueCommand(appointment.Id), CancellationToken.None));

        Assert.Equal("APPOINTMENT_NOT_PHYSICAL", ex.Code);
    }

    [Fact]
    public async Task Join_queue_rejects_unconfirmed_appointment()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync("pending");
        var appointment = await SeedPendingAppointmentAsync(doctor, patient);
        _host.CurrentUser.UserId = patient.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _host.Sender.Send(new JoinQueueCommand(appointment.Id), CancellationToken.None));

        Assert.Equal("APPOINTMENT_NOT_CONFIRMED", ex.Code);
    }

    [Fact]
    public async Task Join_queue_rejects_duplicate_active_entry()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync("duplicate");
        var appointment = await SeedConfirmedAppointmentAsync(doctor, patient);
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(new JoinQueueCommand(appointment.Id), CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _host.Sender.Send(new JoinQueueCommand(appointment.Id), CancellationToken.None));

        Assert.Equal("QUEUE_ENTRY_ALREADY_EXISTS", ex.Code);
    }

    private async Task<Doctor> SeedVerifiedDoctorAsync(DoctorAppointmentType appointmentType = DoctorAppointmentType.Both)
    {
        var command = DoctorRegistrationTestData.CreateValidCommand();
        if (appointmentType != DoctorAppointmentType.Both)
        {
            command = command with
            {
                AvailabilitySlots =
                [
                    new DoctorAvailabilitySlotInput(
                        DayOfWeek.Monday,
                        new TimeOnly(8, 0),
                        new TimeOnly(12, 0),
                        30,
                        appointmentType)
                ]
            };
        }

        var registration = await _host.Sender.Send(command, CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync(string suffix = "default")
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Patient {suffix}",
                null,
                $"patient-{suffix}-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }

    private async Task<Appointment> SeedConfirmedAppointmentAsync(Doctor doctor, Patient patient)
    {
        var appointment = CreateAppointment(doctor, patient);
        appointment.ConfirmOnPayment(DateTime.UtcNow);
        _host.DbContext.Appointments.Add(appointment);
        await _host.DbContext.SaveChangesAsync();
        return appointment;
    }

    private async Task<Appointment> SeedPendingAppointmentAsync(Doctor doctor, Patient patient)
    {
        var appointment = CreateAppointment(doctor, patient);
        _host.DbContext.Appointments.Add(appointment);
        await _host.DbContext.SaveChangesAsync();
        return appointment;
    }

    private static Appointment CreateAppointment(Doctor doctor, Patient patient)
    {
        var slot = doctor.AvailabilitySlots.Single();
        return Appointment.CreatePendingPayment(
            patient.Id,
            doctor.Id,
            slot.Id,
            ConsultationType.General,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddMinutes(10));
    }
}
