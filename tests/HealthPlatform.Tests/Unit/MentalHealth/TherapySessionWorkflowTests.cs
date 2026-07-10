using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.GrantHealthRecordAccess;
using HealthPlatform.Application.HealthRecords.ListHealthRecordEntries;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.MentalHealth;
using HealthPlatform.Application.MentalHealth.CompleteTherapySession;
using HealthPlatform.Application.MentalHealth.GrantTherapySessionBroaderAccess;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.MentalHealth;
using HealthPlatform.Infrastructure.MongoDb;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class TherapySessionWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Therapy_booking_creates_scheduled_session_for_licensed_therapist()
    {
        var therapist = await SeedTherapistAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var result = await _host.Sender.Send(
            new BookAppointmentCommand(
                therapist.Id,
                therapist.AvailabilitySlots.Single().Id,
                DateTime.UtcNow.AddDays(2),
                ConsultationType.Therapy),
            CancellationToken.None);

        Assert.Equal(ConsultationType.Therapy, result.ConsultationType);

        var session = await _host.DbContext.TherapySessions
            .SingleAsync(s => s.AppointmentId == result.AppointmentId);
        Assert.Equal(TherapySessionStatus.Scheduled, session.Status);
        Assert.Equal(therapist.Id, session.TherapistId);
        Assert.Equal(patient.Id, session.PatientId);
    }

    [Fact]
    public async Task Therapy_booking_rejects_non_therapist_doctor()
    {
        var doctor = await SeedGeneralDoctorAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.Single().Id,
                DateTime.UtcNow.AddDays(2),
                ConsultationType.Therapy),
            CancellationToken.None));

        Assert.Equal(TherapySessionErrorCodes.TherapistRequired, ex.Code);
    }

    [Fact]
    public async Task Completion_attaches_summary_visible_to_patient_and_therapist_only()
    {
        var context = await SeedScheduledTherapySessionAsync();
        _host.CurrentUser.UserId = context.Therapist.UserId;

        var completed = await _host.Sender.Send(
            new CompleteTherapySessionCommand(context.Session.Id, "Session focused on anxiety management."),
            CancellationToken.None);

        Assert.Equal("completed", completed.Status);
        Assert.NotNull(completed.SummaryEntryId);

        var entryRepository = _host.GetRequiredService<InMemoryHealthRecordEntryRepository>();
        var summaryEntry = entryRepository.Entries.Single(e => e.Id == completed.SummaryEntryId);
        Assert.Equal(HealthRecordEntryType.TherapySessionSummary, summaryEntry.EntryType);
        Assert.True(summaryEntry.IsVisibleToPatient);
        Assert.False(summaryEntry.Content.TherapySessionSummary!.BroaderAccessGranted);

        var summaryRepository = _host.GetRequiredService<InMemoryTherapySessionSummaryRepository>();
        Assert.Single(summaryRepository.Summaries);
    }

    [Fact]
    public async Task Broader_access_allows_other_doctors_to_view_summary()
    {
        var context = await SeedCompletedTherapySessionAsync();
        var otherDoctor = await SeedGeneralDoctorAsync();
        var healthRecord = await _host.DbContext.HealthRecords
            .SingleAsync(r => r.PatientId == context.Patient.Id);

        _host.CurrentUser.UserId = context.Patient.UserId;
        await _host.Sender.Send(
            new GrantHealthRecordAccessCommand(otherDoctor.Id, HealthRecordAccessType.Full, null),
            CancellationToken.None);

        _host.CurrentUser.UserId = otherDoctor.UserId;
        var beforeGrant = await _host.Sender.Send(
            new ListHealthRecordEntriesQuery(healthRecord.Id),
            CancellationToken.None);
        Assert.DoesNotContain(
            beforeGrant,
            entry => entry.EntryType == HealthRecordEntryType.TherapySessionSummary);

        _host.CurrentUser.UserId = context.Patient.UserId;
        await _host.Sender.Send(
            new GrantTherapySessionBroaderAccessCommand(context.Session.Id),
            CancellationToken.None);

        _host.CurrentUser.UserId = otherDoctor.UserId;
        var afterGrant = await _host.Sender.Send(
            new ListHealthRecordEntriesQuery(healthRecord.Id),
            CancellationToken.None);
        Assert.Contains(
            afterGrant,
            entry => entry.EntryType == HealthRecordEntryType.TherapySessionSummary
                && entry.Content.TherapySessionSummary!.BroaderAccessGranted);
    }

    [Fact]
    public async Task Completion_requires_assigned_therapist()
    {
        var context = await SeedScheduledTherapySessionAsync();
        var outsider = await SeedGeneralDoctorAsync();
        _host.CurrentUser.UserId = outsider.UserId;

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(() => _host.Sender.Send(
            new CompleteTherapySessionCommand(context.Session.Id, "Summary."),
            CancellationToken.None));

        Assert.Equal(TherapySessionErrorCodes.TherapistAccessDenied, ex.Code);
    }

    private async Task<(TherapySession Session, Doctor Therapist, Patient Patient)> SeedScheduledTherapySessionAsync()
    {
        var therapist = await SeedTherapistAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                therapist.Id,
                therapist.AvailabilitySlots.Single().Id,
                DateTime.UtcNow.AddDays(2),
                ConsultationType.Therapy),
            CancellationToken.None);

        var session = await _host.DbContext.TherapySessions
            .SingleAsync(s => s.AppointmentId == booking.AppointmentId);

        return (session, therapist, patient);
    }

    private async Task<(TherapySession Session, Doctor Therapist, Patient Patient)> SeedCompletedTherapySessionAsync()
    {
        var context = await SeedScheduledTherapySessionAsync();
        _host.CurrentUser.UserId = context.Therapist.UserId;
        await _host.Sender.Send(
            new CompleteTherapySessionCommand(context.Session.Id, "Completed session summary."),
            CancellationToken.None);

        var completed = await _host.DbContext.TherapySessions.SingleAsync(s => s.Id == context.Session.Id);
        return (completed, context.Therapist, context.Patient);
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

    private async Task<Doctor> SeedGeneralDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        return await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync(string suffix = "default")
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Patient {suffix}",
                null,
                $"patient-{suffix}-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
