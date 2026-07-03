using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.NextOfKin;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.NextOfKin;

public sealed class NextOfKinControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task CreateAsync_returns_created_contact_and_notifies_designee()
    {
        var patientUserId = await RegisterPatientAsync();
        _host.CurrentUser.UserId = patientUserId;

        var controller = new NextOfKinController(_host.Sender);
        var result = await controller.CreateAsync(
            new NextOfKinContactUpsertRequest
            {
                FullName = "Controller Kin",
                Relationship = "Parent",
                PhoneNumber = "+263771234567",
                Email = "kin@example.com",
                IsMentalHealthContact = false
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var contact = Assert.IsType<NextOfKinContactDto>(created.Value);
        Assert.Equal("Controller Kin", contact.FullName);
        Assert.Single(_host.NextOfKinDesignationNotifier.Calls);
    }

    [Fact]
    public async Task ListAsync_returns_patient_contacts()
    {
        var patientUserId = await RegisterPatientAsync();
        _host.CurrentUser.UserId = patientUserId;

        var controller = new NextOfKinController(_host.Sender);
        await controller.CreateAsync(
            new NextOfKinContactUpsertRequest
            {
                FullName = "Listed Kin",
                Relationship = "Sibling",
                PhoneNumber = "+263771234568",
                IsMentalHealthContact = true
            },
            CancellationToken.None);

        var result = await controller.ListAsync(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var contacts = Assert.IsAssignableFrom<IReadOnlyList<NextOfKinContactDto>>(ok.Value);
        Assert.Single(contacts);
        Assert.Equal("Listed Kin", contacts[0].FullName);
        Assert.True(contacts[0].IsMentalHealthContact);
    }

    private async Task<Guid> RegisterPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Controller Next Of Kin Patient",
                null,
                $"controller-nok-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).First();
        return patient.UserId;
    }
}
