using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class BookAppointmentCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Booking_creates_pending_payment_appointment()
    {
        var (doctor, slotId) = await SeedDoctorWithSlotAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var scheduledAtUtc = DateTime.UtcNow.AddDays(2);
        var result = await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, slotId, scheduledAtUtc),
            CancellationToken.None);

        Assert.Equal("pending_payment", result.Status);

        var appointment = await _host.DbContext.Appointments.SingleAsync(a => a.Id == result.AppointmentId);
        Assert.Equal(doctor.Id, appointment.DoctorId);
        Assert.Equal(patient.Id, appointment.PatientId);
    }

    [Fact]
    public async Task Booking_conflicts_when_slot_is_already_held()
    {
        var (doctor, slotId) = await SeedDoctorWithSlotAsync();
        var patientOne = await SeedPatientAsync("first");
        var patientTwo = await SeedPatientAsync("second");

        _host.CurrentUser.UserId = patientOne.UserId;
        await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, slotId, DateTime.UtcNow.AddDays(1)),
            CancellationToken.None);

        _host.CurrentUser.UserId = patientTwo.UserId;
        var ex = await Assert.ThrowsAsync<ConflictException>(() => _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, slotId, DateTime.UtcNow.AddDays(1)),
            CancellationToken.None));

        Assert.Equal(AppointmentErrorCodes.SlotUnavailable, ex.Code);
    }

    private async Task<(Domain.Identity.Doctor Doctor, Guid SlotId)> SeedDoctorWithSlotAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == registration.DoctorId);

        return (doctor, doctor.AvailabilitySlots.Single().Id);
    }

    private async Task<Domain.Identity.Patient> SeedPatientAsync(string suffix = "default")
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
}
