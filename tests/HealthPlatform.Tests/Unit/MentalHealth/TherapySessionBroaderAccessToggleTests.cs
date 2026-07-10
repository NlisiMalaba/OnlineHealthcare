using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.MentalHealth;
using HealthPlatform.Application.MentalHealth.CompleteTherapySession;
using HealthPlatform.Application.MentalHealth.GrantTherapySessionBroaderAccess;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.MentalHealth;
using HealthPlatform.Infrastructure.MongoDb;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class TherapySessionBroaderAccessToggleTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Broader_access_grant_toggles_session_and_summary_flags()
    {
        var context = await SeedCompletedTherapySessionAsync();

        _host.CurrentUser.UserId = context.Patient.UserId;
        var granted = await _host.Sender.Send(
            new GrantTherapySessionBroaderAccessCommand(context.Session.Id),
            CancellationToken.None);

        Assert.True(granted.BroaderAccessGranted);

        var persistedSession = await _host.DbContext.TherapySessions.SingleAsync(s => s.Id == context.Session.Id);
        Assert.True(persistedSession.BroaderAccessGranted);

        var entryRepository = _host.GetRequiredService<InMemoryHealthRecordEntryRepository>();
        var summary = entryRepository.Entries.Single(entry => entry.Id == granted.SummaryEntryId);
        Assert.Equal(HealthRecordEntryType.TherapySessionSummary, summary.EntryType);
        Assert.True(summary.Content.TherapySessionSummary!.BroaderAccessGranted);
    }

    [Fact]
    public async Task Broader_access_grant_is_idempotent_for_already_granted_session()
    {
        var context = await SeedCompletedTherapySessionAsync();
        _host.CurrentUser.UserId = context.Patient.UserId;

        await _host.Sender.Send(
            new GrantTherapySessionBroaderAccessCommand(context.Session.Id),
            CancellationToken.None);
        var secondGrant = await _host.Sender.Send(
            new GrantTherapySessionBroaderAccessCommand(context.Session.Id),
            CancellationToken.None);

        Assert.True(secondGrant.BroaderAccessGranted);
        Assert.True(await _host.DbContext.TherapySessions
            .Where(session => session.Id == context.Session.Id)
            .Select(session => session.BroaderAccessGranted)
            .SingleAsync());
    }

    [Fact]
    public async Task Broader_access_grant_rejected_for_scheduled_session()
    {
        var context = await SeedScheduledTherapySessionAsync();
        _host.CurrentUser.UserId = context.Patient.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new GrantTherapySessionBroaderAccessCommand(context.Session.Id),
            CancellationToken.None));

        Assert.Equal(TherapySessionErrorCodes.TherapySessionBroaderAccessNotAllowed, ex.Code);
        Assert.False(await _host.DbContext.TherapySessions
            .Where(session => session.Id == context.Session.Id)
            .Select(session => session.BroaderAccessGranted)
            .SingleAsync());
    }

    private async Task<(TherapySession Session, Patient Patient, Guid HealthRecordId)> SeedScheduledTherapySessionAsync()
    {
        var therapist = await SeedTherapistAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var booking = await _host.Sender.Send(
            new Application.Appointments.BookAppointment.BookAppointmentCommand(
                therapist.Id,
                therapist.AvailabilitySlots.Single().Id,
                DateTime.UtcNow.AddDays(2),
                Domain.Appointments.ConsultationType.Therapy),
            CancellationToken.None);

        var session = await _host.DbContext.TherapySessions
            .SingleAsync(s => s.AppointmentId == booking.AppointmentId);
        var healthRecordId = await _host.DbContext.HealthRecords
            .Where(record => record.PatientId == patient.Id)
            .Select(record => record.Id)
            .SingleAsync();

        return (session, patient, healthRecordId);
    }

    private async Task<(TherapySession Session, Patient Patient, Guid HealthRecordId)> SeedCompletedTherapySessionAsync()
    {
        var context = await SeedScheduledTherapySessionAsync();
        var therapist = await _host.DbContext.Doctors.SingleAsync(d => d.Id == context.Session.TherapistId);

        _host.CurrentUser.UserId = therapist.UserId;
        await _host.Sender.Send(
            new CompleteTherapySessionCommand(context.Session.Id, "Completed summary for broader access."),
            CancellationToken.None);

        var completed = await _host.DbContext.TherapySessions.SingleAsync(s => s.Id == context.Session.Id);
        return (completed, context.Patient, context.HealthRecordId);
    }

    private async Task<Doctor> SeedTherapistAsync()
    {
        var registration = await _host.Sender.Send(
            TherapistRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        return await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Broader Access Patient",
                null,
                $"broader-access-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }
}
