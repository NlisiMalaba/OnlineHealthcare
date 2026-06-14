using HealthPlatform.API.Controllers;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.Identity;

public sealed class IdentityControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task RegisterPatient_ReturnsCreatedWithHealthRecord()
    {
        var controller = new IdentityController(_host.Sender);
        var command = new RegisterPatientCommand(
            PatientAuthProvider.Email,
            "Controller Patient",
            null,
            $"controller-{Guid.NewGuid():N}@example.com",
            PatientRegistrationTestHost.ValidPassword,
            null);

        var result = await controller.RegisterPatientAsync(command, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var payload = Assert.IsType<PatientRegistrationResponseDto>(created.Value);
        Assert.NotEqual(Guid.Empty, payload.PatientId);
        Assert.NotEqual(Guid.Empty, payload.HealthRecordId);
    }

    [Fact]
    public async Task RegisterPatient_WithDuplicateEmail_ThrowsIdentityConflict()
    {
        var controller = new IdentityController(_host.Sender);
        var email = $"duplicate-controller-{Guid.NewGuid():N}@example.com";
        var command = new RegisterPatientCommand(
            PatientAuthProvider.Email,
            "Duplicate Controller Patient",
            null,
            email,
            PatientRegistrationTestHost.ValidPassword,
            null);

        await controller.RegisterPatientAsync(command, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            controller.RegisterPatientAsync(command, CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.IdentityConflict, ex.Code);
    }
}
