using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.EventHandlers;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Support;

public sealed record TelemedicineSessionTestContext(
    Guid AppointmentId,
    Guid PatientUserId,
    Guid DoctorUserId);

public static class TelemedicineSessionTestContextFactory
{
    public static async Task<TelemedicineSessionTestContext> CreateAsync(PatientRegistrationTestHost host)
    {
        var doctorRegistration = await host.Sender.Send(
            TelemedicineTestData.CreateVirtualDoctorCommand(),
            CancellationToken.None);

        var doctor = await host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Telemedicine Session Patient",
                null,
                $"telemed-session-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        host.CurrentUser.UserId = patient.UserId;

        var booking = await host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.Single().Id,
                DateTime.UtcNow.AddDays(1)),
            CancellationToken.None);

        await new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            host.GetRequiredService<IAppointmentRepository>(),
            host.GetRequiredService<IOutboxRepository>(),
            host.GetRequiredService<IDomainEventPublisher>())
            .Handle(
                new PaymentCompletedNotification(booking.AppointmentId, Guid.CreateVersion7(), DateTime.UtcNow),
                CancellationToken.None);

        var appointment = await host.DbContext.Appointments.SingleAsync(x => x.Id == booking.AppointmentId);

        await new CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler(
            host.GetRequiredService<IAppointmentRepository>(),
            host.GetRequiredService<IDoctorRepository>(),
            host.GetRequiredService<ITelemedicineSessionRepository>(),
            host.GetRequiredService<IRtcProviderResolver>(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler>.Instance)
            .Handle(
                new AppointmentConfirmedNotification(
                    appointment.Id,
                    appointment.PatientId,
                    appointment.DoctorId,
                    appointment.ScheduledAtUtc,
                    DateTime.UtcNow,
                    DateTime.UtcNow),
                CancellationToken.None);

        return new TelemedicineSessionTestContext(booking.AppointmentId, patient.UserId, doctor.UserId);
    }

    public static async Task<TelemedicineSessionTestContext> CreateActiveAsync(PatientRegistrationTestHost host)
    {
        var context = await CreateAsync(host);
        host.CurrentUser.UserId = context.PatientUserId;

        await host.Sender.Send(
            new Application.Telemedicine.JoinSession.JoinTelemedicineSessionCommand(context.AppointmentId, null),
            CancellationToken.None);

        return context;
    }
}
