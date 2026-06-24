using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.AvailabilitySlots;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class DoctorAvailabilitySlotHandlersTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_slot_adds_new_availability_slot()
    {
        await RegisterDoctorAndSetCurrentUserAsync();

        var created = await _host.Sender.Send(
            new CreateDoctorAvailabilitySlotCommand(
                DayOfWeek.Tuesday,
                new TimeOnly(13, 0),
                new TimeOnly(17, 0),
                30,
                DoctorAppointmentType.Virtual),
            CancellationToken.None);

        Assert.Equal(DayOfWeek.Tuesday, created.DayOfWeek);

        var doctor = await _host.DbContext.Doctors.Include(d => d.AvailabilitySlots).SingleAsync();
        Assert.Contains(doctor.AvailabilitySlots, slot => slot.Id == created.Id);
    }

    [Fact]
    public async Task Update_slot_updates_existing_slot_values()
    {
        await RegisterDoctorAndSetCurrentUserAsync();
        var doctor = await _host.DbContext.Doctors.Include(d => d.AvailabilitySlots).SingleAsync();
        var slotId = doctor.AvailabilitySlots.Single().Id;

        var updated = await _host.Sender.Send(
            new UpdateDoctorAvailabilitySlotCommand(
                slotId,
                DayOfWeek.Saturday,
                new TimeOnly(9, 0),
                new TimeOnly(12, 0),
                20,
                DoctorAppointmentType.Both),
            CancellationToken.None);

        Assert.Equal(slotId, updated.Id);
        Assert.Equal(DayOfWeek.Saturday, updated.DayOfWeek);
        Assert.Equal(20, updated.SlotDurationMinutes);
    }

    [Fact]
    public async Task Delete_slot_removes_slot()
    {
        await RegisterDoctorAndSetCurrentUserAsync();
        var doctor = await _host.DbContext.Doctors.Include(d => d.AvailabilitySlots).SingleAsync();
        var slotId = doctor.AvailabilitySlots.Single().Id;

        await _host.Sender.Send(new DeleteDoctorAvailabilitySlotCommand(slotId), CancellationToken.None);

        var afterDelete = await _host.DbContext.Doctors.Include(d => d.AvailabilitySlots).SingleAsync();
        Assert.DoesNotContain(afterDelete.AvailabilitySlots, slot => slot.Id == slotId);
    }

    [Fact]
    public async Task Get_slot_returns_not_found_for_unknown_slot()
    {
        await RegisterDoctorAndSetCurrentUserAsync();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _host.Sender.Send(
            new GetDoctorAvailabilitySlotQuery(Guid.CreateVersion7()),
            CancellationToken.None));

        Assert.Equal(AppointmentErrorCodes.AvailabilitySlotNotFound, ex.Code);
    }

    [Fact]
    public async Task List_slots_returns_ordered_slots()
    {
        await RegisterDoctorAndSetCurrentUserAsync();
        await _host.Sender.Send(
            new CreateDoctorAvailabilitySlotCommand(
                DayOfWeek.Friday,
                new TimeOnly(14, 0),
                new TimeOnly(16, 0),
                30,
                DoctorAppointmentType.Physical),
            CancellationToken.None);

        var slots = await _host.Sender.Send(new ListDoctorAvailabilitySlotsQuery(), CancellationToken.None);

        Assert.True(slots.Count >= 2);
        Assert.True(slots[0].DayOfWeek <= slots[1].DayOfWeek);
    }

    private async Task RegisterDoctorAndSetCurrentUserAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        _host.CurrentUser.UserId = doctor.UserId;
    }
}
