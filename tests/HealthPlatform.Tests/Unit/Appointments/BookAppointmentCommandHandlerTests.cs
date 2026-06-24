using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.AvailabilitySlots;
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

    [Fact]
    public async Task Booking_physical_slot_includes_clinic_address_and_navigation_link()
    {
        var (doctor, slotId) = await SeedDoctorWithSlotAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var result = await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, slotId, DateTime.UtcNow.AddDays(2)),
            CancellationToken.None);

        Assert.Equal(DoctorAppointmentType.Both, result.AppointmentType);
        Assert.NotNull(result.Clinic);
        Assert.Equal(doctor.ClinicAddress, result.Clinic.Address);
        Assert.Equal(doctor.ClinicLocation!.Latitude, result.Clinic.Latitude);
        Assert.Equal(doctor.ClinicLocation.Longitude, result.Clinic.Longitude);
        Assert.Contains("google.com/maps/dir", result.Clinic.GpsNavigationUrl);
    }

    [Fact]
    public async Task Booking_virtual_slot_omits_clinic_details()
    {
        var (doctor, _) = await SeedDoctorWithSlotAsync();
        _host.CurrentUser.UserId = doctor.UserId;

        var virtualSlot = await _host.Sender.Send(
            new CreateDoctorAvailabilitySlotCommand(
                DayOfWeek.Friday,
                new TimeOnly(14, 0),
                new TimeOnly(16, 0),
                30,
                DoctorAppointmentType.Virtual),
            CancellationToken.None);

        var patient = await SeedPatientAsync("virtual");
        _host.CurrentUser.UserId = patient.UserId;

        var result = await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, virtualSlot.Id, DateTime.UtcNow.AddDays(3)),
            CancellationToken.None);

        Assert.Equal(DoctorAppointmentType.Virtual, result.AppointmentType);
        Assert.Null(result.Clinic);
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
