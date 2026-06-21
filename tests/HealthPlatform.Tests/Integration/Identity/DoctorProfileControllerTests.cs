using System.Text.Json;
using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Identity;

public sealed class DoctorProfileControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task UpdateProfile_ReturnsUpdatedDoctorProfile()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        _host.CurrentUser.UserId = doctor.UserId;

        var controller = new DoctorProfileController(_host.Sender);
        var result = await controller.UpdateProfileAsync(
            new UpdateDoctorProfileRequest
            {
                VirtualFee = 42m,
                PhysicalFee = 58m,
                Bio = "Controller updated bio.",
                AvailabilitySlotsJson = JsonSerializer.Serialize(new[]
                {
                    new
                    {
                        dayOfWeek = DayOfWeek.Thursday,
                        startTime = "08:00",
                        endTime = "12:00",
                        slotDurationMinutes = 30,
                        appointmentType = DoctorAppointmentType.Physical
                    }
                })
            },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var profile = Assert.IsType<DoctorProfileDto>(ok.Value);
        Assert.Equal(42m, profile.VirtualFee);
        Assert.Equal(58m, profile.PhysicalFee);
        Assert.Equal("Controller updated bio.", profile.Bio);
        Assert.Single(profile.AvailabilitySlots);
        Assert.Equal(DayOfWeek.Thursday, profile.AvailabilitySlots[0].DayOfWeek);
    }
}
