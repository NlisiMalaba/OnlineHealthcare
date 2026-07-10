using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Appointments;
using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.MentalHealth.CompleteTherapySession;
using HealthPlatform.Application.MentalHealth.GrantTherapySessionBroaderAccess;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.MentalHealth;

public sealed class TherapySessionsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Book_therapy_endpoint_creates_pending_payment_appointment()
    {
        var therapist = await SeedTherapistAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var appointmentsController = new AppointmentsController(_host.Sender);
        var created = await appointmentsController.BookAsync(
            new BookAppointmentRequest
            {
                DoctorId = therapist.Id,
                SlotId = therapist.AvailabilitySlots.Single().Id,
                ScheduledAtUtc = DateTime.UtcNow.AddDays(1),
                ConsultationType = ConsultationType.Therapy
            },
            CancellationToken.None);

        var result = Assert.IsType<CreatedResult>(created.Result);
        var payload = Assert.IsType<BookAppointmentDto>(result.Value);
        Assert.Equal(ConsultationType.Therapy, payload.ConsultationType);
        Assert.Equal("pending_payment", payload.Status);

        var session = await _host.DbContext.TherapySessions
            .SingleAsync(s => s.AppointmentId == payload.AppointmentId);
        Assert.Equal(therapist.Id, session.TherapistId);
    }

    [Fact]
    public async Task Complete_endpoint_attaches_session_summary()
    {
        var context = await SeedScheduledTherapySessionAsync();
        _host.CurrentUser.UserId = context.Therapist.UserId;

        var controller = new TherapySessionsController(_host.Sender);
        var result = await controller.CompleteAsync(
            context.Session.Id,
            new CompleteTherapySessionRequest { SessionSummary = "Discussed coping techniques." },
            CancellationToken.None);

        var payload = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<Application.MentalHealth.TherapySessionDto>(payload.Value);
        Assert.Equal("completed", session.Status);
        Assert.NotNull(session.SummaryEntryId);
    }

    [Fact]
    public async Task Grant_broader_access_endpoint_updates_session()
    {
        var context = await SeedCompletedTherapySessionAsync();
        _host.CurrentUser.UserId = context.Patient.UserId;

        var controller = new TherapySessionsController(_host.Sender);
        var result = await controller.GrantBroaderAccessAsync(context.Session.Id, CancellationToken.None);

        var payload = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<Application.MentalHealth.TherapySessionDto>(payload.Value);
        Assert.True(session.BroaderAccessGranted);
    }

    private async Task<(Domain.MentalHealth.TherapySession Session, Doctor Therapist, Patient Patient)> SeedScheduledTherapySessionAsync()
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

    private async Task<(Domain.MentalHealth.TherapySession Session, Doctor Therapist, Patient Patient)> SeedCompletedTherapySessionAsync()
    {
        var context = await SeedScheduledTherapySessionAsync();
        _host.CurrentUser.UserId = context.Therapist.UserId;
        await _host.Sender.Send(
            new CompleteTherapySessionCommand(context.Session.Id, "Completed."),
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

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Therapy Patient",
                null,
                $"therapy-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
