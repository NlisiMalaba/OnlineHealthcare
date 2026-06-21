using System.Text;
using System.Text.Json;
using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace HealthPlatform.Tests.Integration.Identity;

public sealed class DoctorIdentityControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task RegisterDoctor_ReturnsCreatedWithPendingStatus()
    {
        var controller = CreateController();
        var credentials = CreateFormFile("license.pdf", "application/pdf", [0x25, 0x50, 0x44, 0x46]);

        var request = new RegisterDoctorRequest
        {
            FullName = "Dr. Controller Test",
            LicenseNumber = $"HPCZ-CTRL-{Guid.NewGuid():N}"[..20],
            Specialty = "Cardiology",
            YearsOfExperience = 12,
            ClinicAddress = "45 Leopold Takawira, Bulawayo",
            ClinicLatitude = -20.1555,
            ClinicLongitude = 28.5845,
            VirtualFee = 35m,
            PhysicalFee = 55m,
            Email = $"doctor-controller-{Guid.NewGuid():N}@example.com",
            PhoneNumber = $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}",
            Password = PatientRegistrationTestHost.ValidPassword,
            AvailabilitySlotsJson = JsonSerializer.Serialize(new[]
            {
                new
                {
                    dayOfWeek = DayOfWeek.Wednesday,
                    startTime = "09:00",
                    endTime = "13:00",
                    slotDurationMinutes = 30,
                    appointmentType = DoctorAppointmentType.Virtual
                }
            }),
            Credentials = credentials
        };

        var result = await controller.RegisterDoctorAsync(request, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var payload = Assert.IsType<DoctorRegistrationResponseDto>(created.Value);
        Assert.Equal("pending", payload.VerificationStatus);
        Assert.NotEqual(Guid.Empty, payload.DoctorId);
    }

    private DoctorIdentityController CreateController()
    {
        var controller = new DoctorIdentityController(_host.Sender)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }

    private static FormFile CreateFormFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary
            {
                ["Content-Type"] = new StringValues(contentType)
            }
        };
    }
}
