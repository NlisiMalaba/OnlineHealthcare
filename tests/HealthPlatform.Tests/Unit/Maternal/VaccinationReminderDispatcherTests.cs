using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Vaccinations;
using HealthPlatform.Infrastructure.Vaccinations;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class VaccinationReminderDispatcherTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task DispatchDueRemindersAsync_notifies_guardian_seven_days_before_due_date()
    {
        var guardian = await SeedGuardianAsync();
        _host.CurrentUser.UserId = guardian.UserId;
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));

        var profile = await _host.Sender.Send(
            new CreateChildProfileCommand("Reminder Child", dateOfBirth, null, []),
            CancellationToken.None);

        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var entry = VaccinationScheduleEntry.CreateForChild(
            profile.Id,
            "Test Vaccine",
            dueDate,
            "Test dose",
            DateTime.UtcNow);
        await _host.DbContext.VaccinationScheduleEntries.AddAsync(entry);
        await _host.DbContext.SaveChangesAsync();

        var notifier = new Mock<IVaccinationReminderNotifier>();
        var dispatcher = new VaccinationReminderDispatcher(
            TimeProvider.System,
            _host.GetRequiredService<IVaccinationScheduleRepository>(),
            _host.GetRequiredService<IPatientRepository>(),
            _host.GetRequiredService<IChildProfileRepository>(),
            notifier.Object,
            NullLogger<VaccinationReminderDispatcher>.Instance);

        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, dispatched);
        notifier.Verify(
            n => n.NotifyVaccinationDueAsync(
                guardian.UserId,
                profile.Id,
                null,
                entry.Id,
                "Test Vaccine",
                dueDate,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private async Task<Patient> SeedGuardianAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Reminder Guardian",
                null,
                $"reminder-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
