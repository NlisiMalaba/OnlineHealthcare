using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Appointments;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Appointments;

public sealed class DoctorAvailabilitySlotsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Crud_endpoints_manage_doctor_availability_slots()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        _host.CurrentUser.UserId = doctor.UserId;

        var controller = new DoctorAvailabilitySlotsController(_host.Sender);
        var createRequest = new DoctorAvailabilitySlotUpsertRequest
        {
            DayOfWeek = DayOfWeek.Sunday,
            StartTime = "09:00",
            EndTime = "11:00",
            SlotDurationMinutes = 30,
            AppointmentType = DoctorAppointmentType.Virtual
        };

        var createdResult = await controller.CreateAsync(createRequest, CancellationToken.None);
        var created = Assert.IsType<CreatedAtActionResult>(createdResult.Result);
        var createdSlot = Assert.IsType<DoctorAvailabilitySlotDto>(created.Value);

        var updatedResult = await controller.UpdateAsync(
            createdSlot.Id,
            new DoctorAvailabilitySlotUpsertRequest
            {
                DayOfWeek = DayOfWeek.Sunday,
                StartTime = "10:00",
                EndTime = "12:00",
                SlotDurationMinutes = 20,
                AppointmentType = DoctorAppointmentType.Both
            },
            CancellationToken.None);

        var updated = Assert.IsType<OkObjectResult>(updatedResult.Result);
        var updatedSlot = Assert.IsType<DoctorAvailabilitySlotDto>(updated.Value);
        Assert.Equal(20, updatedSlot.SlotDurationMinutes);

        var listResult = await controller.ListAsync(CancellationToken.None);
        var list = Assert.IsType<OkObjectResult>(listResult.Result);
        var slots = Assert.IsAssignableFrom<IReadOnlyList<DoctorAvailabilitySlotDto>>(list.Value);
        Assert.Contains(slots, slot => slot.Id == createdSlot.Id);

        var deleteResult = await controller.DeleteAsync(createdSlot.Id, CancellationToken.None);
        Assert.IsType<NoContentResult>(deleteResult);
    }
}
