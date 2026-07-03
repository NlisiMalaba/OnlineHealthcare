using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.NextOfKin.AddNextOfKinContact;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Tests.Support;
using Xunit;

namespace HealthPlatform.Tests.Unit.NextOfKin;

public sealed class AddNextOfKinContactCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Handle_creates_contact_and_notifies_designated_contact()
    {
        var patientUserId = await RegisterPatientAsync();
        _host.CurrentUser.UserId = patientUserId;

        var contact = await _host.Sender.Send(
            new AddNextOfKinContactCommand(
                "Jane Kin",
                "Sister",
                "+263771234567",
                "jane.kin@example.com",
                true),
            CancellationToken.None);

        Assert.Equal("Jane Kin", contact.FullName);
        Assert.True(contact.IsMentalHealthContact);
        Assert.Single(_host.NextOfKinDesignationNotifier.Calls);
        Assert.Equal(contact.Id, _host.NextOfKinDesignationNotifier.Calls[0].ContactId);
    }

    [Fact]
    public async Task Handle_rejects_fourth_contact_with_conflict()
    {
        var patientUserId = await RegisterPatientAsync();
        _host.CurrentUser.UserId = patientUserId;

        for (var index = 0; index < 3; index++)
        {
            await _host.Sender.Send(
                new AddNextOfKinContactCommand(
                    $"Contact {index}",
                    "Sibling",
                    $"+26377123456{index}",
                    null,
                    false),
                CancellationToken.None);
        }

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _host.Sender.Send(
            new AddNextOfKinContactCommand(
                "Fourth Contact",
                "Cousin",
                "+263771234569",
                null,
                false),
            CancellationToken.None));

        Assert.Equal("NEXT_OF_KIN_MAX_CONTACTS_REACHED", ex.Code);
    }

    private async Task<Guid> RegisterPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Next Of Kin Patient",
                null,
                $"next-of-kin-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).First();
        return patient.UserId;
    }
}
