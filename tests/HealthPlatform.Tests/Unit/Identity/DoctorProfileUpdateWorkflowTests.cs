using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.UpdateDoctorProfile;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class DoctorProfileUpdateWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task UpdateProfile_UpdatesFeesAndBio()
    {
        var doctorUserId = await RegisterDoctorAndSetCurrentUserAsync();

        var profile = await _host.Sender.Send(
            new UpdateDoctorProfileCommand(
                55m,
                75m,
                "Updated professional bio.",
                null,
                null,
                null),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.UserId == doctorUserId);

        Assert.Equal(55m, profile.VirtualFee);
        Assert.Equal(75m, profile.PhysicalFee);
        Assert.Equal("Updated professional bio.", profile.Bio);
        Assert.Equal(55m, doctor.VirtualFee);
        Assert.Equal(75m, doctor.PhysicalFee);
        Assert.Empty(_host.SearchService.AvailabilityUpdates);
    }

    [Fact]
    public async Task UpdateProfile_ReplacesAvailabilityAndUpdatesSearchIndexForVerifiedDoctor()
    {
        var doctorUserId = await RegisterDoctorAndSetCurrentUserAsync();
        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.UserId == doctorUserId);

        await _host.Sender.Send(new VerifyDoctorLicenseCommand(doctor.Id), CancellationToken.None);
        _host.SearchService.AvailabilityUpdates.Clear();

        var replacementSlots = new List<DoctorAvailabilitySlotInput>
        {
            new(
                DayOfWeek.Friday,
                new TimeOnly(10, 0),
                new TimeOnly(14, 0),
                45,
                DoctorAppointmentType.Virtual)
        };

        var profile = await _host.Sender.Send(
            new UpdateDoctorProfileCommand(
                null,
                null,
                null,
                replacementSlots,
                null,
                null),
            CancellationToken.None);

        Assert.Single(profile.AvailabilitySlots);
        Assert.Equal(DayOfWeek.Friday, profile.AvailabilitySlots[0].DayOfWeek);
        Assert.Equal(45, profile.AvailabilitySlots[0].SlotDurationMinutes);

        var outboxEvent = await _host.DbContext.DomainEventOutbox
            .AsNoTracking()
            .SingleAsync(x => x.EventType.Contains("DoctorAvailabilityChangedDomainEvent"));

        Assert.NotNull(outboxEvent);

        Assert.Single(_host.SearchService.AvailabilityUpdates);
        Assert.Equal(doctor.Id, _host.SearchService.AvailabilityUpdates[0].DoctorId);
        Assert.Single(_host.SearchService.AvailabilityUpdates[0].Slots);
    }

    [Fact]
    public async Task UpdateProfile_DoesNotUpdateSearchIndexWhenDoctorIsPending()
    {
        var doctorUserId = await RegisterDoctorAndSetCurrentUserAsync();
        _host.SearchService.AvailabilityUpdates.Clear();

        await _host.Sender.Send(
            new UpdateDoctorProfileCommand(
                null,
                null,
                null,
                [
                    new DoctorAvailabilitySlotInput(
                        DayOfWeek.Tuesday,
                        new TimeOnly(7, 0),
                        new TimeOnly(11, 0),
                        30,
                        DoctorAppointmentType.Both)
                ],
                null,
                null),
            CancellationToken.None);

        Assert.Empty(_host.SearchService.AvailabilityUpdates);
    }

    private async Task<Guid> RegisterDoctorAndSetCurrentUserAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        _host.CurrentUser.UserId = doctor.UserId;
        return doctor.UserId;
    }
}
