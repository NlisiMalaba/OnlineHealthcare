using FsCheck.Xunit;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.NextOfKin.DispatchSystemEmergencyAlert;
using HealthPlatform.Application.NextOfKin.SendDoctorEmergencyAlert;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class EmergencyAlertReachesAllNextOfKinPropertyTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 22: Emergency Alert Reaches All Next of Kin
    [Property(Arbitrary = [typeof(WellnessArbitraries)], MaxTest = 100)]
    public bool Emergency_alert_reaches_all_next_of_kin(EmergencyAlertCase input) =>
        RunEmergencyAlertInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunEmergencyAlertInvariantAsync(EmergencyAlertCase input)
    {
        var clock = new FakeTimeProvider(ReferenceNowUtc);
        await using var host = new PatientRegistrationTestHost(timeProvider: clock);

        var patient = await SeedPatientAsync(host);
        await SeedNextOfKinAsync(host, patient.Id, input.NextOfKinContactCount);

        var alert = input.Trigger == EmergencyAlertTrigger.Doctor
            ? await TriggerDoctorAlertAsync(host, clock, patient)
            : await TriggerSystemAlertAsync(host, patient.Id);

        var expectedContactIds = (await host.DbContext.NextOfKinContacts
            .AsNoTracking()
            .Where(contact => contact.PatientId == patient.Id)
            .Select(contact => contact.Id)
            .ToListAsync())
            .ToHashSet();

        if (expectedContactIds.Count != input.NextOfKinContactCount)
        {
            return false;
        }

        if (!NotifierReachedAllContacts(host, alert.Id, expectedContactIds))
        {
            return false;
        }

        if (!DtoRecordsEveryContact(alert, expectedContactIds))
        {
            return false;
        }

        return await PersistedDeliveriesCoverEveryContactAsync(host, alert.Id, expectedContactIds);
    }

    private static bool NotifierReachedAllContacts(
        PatientRegistrationTestHost host,
        Guid alertId,
        IReadOnlyCollection<Guid> expectedContactIds)
    {
        var call = host.NextOfKinEmergencyAlertNotifier.Calls
            .SingleOrDefault(dispatch => dispatch.EmergencyAlertId == alertId);
        if (call is null)
        {
            return false;
        }

        var notifiedContactIds = call.ContactIds.ToHashSet();
        return notifiedContactIds.Count == expectedContactIds.Count
            && expectedContactIds.All(notifiedContactIds.Contains);
    }

    private static bool DtoRecordsEveryContact(EmergencyAlertDto alert, IReadOnlyCollection<Guid> expectedContactIds)
    {
        var deliveredContactIds = alert.ContactDeliveries
            .Select(delivery => delivery.NextOfKinContactId)
            .ToHashSet();

        return deliveredContactIds.Count == expectedContactIds.Count
            && expectedContactIds.All(deliveredContactIds.Contains);
    }

    private static async Task<bool> PersistedDeliveriesCoverEveryContactAsync(
        PatientRegistrationTestHost host,
        Guid alertId,
        IReadOnlyCollection<Guid> expectedContactIds)
    {
        var persistedContactIds = (await host.DbContext.EmergencyAlertContactDeliveries
            .AsNoTracking()
            .Where(delivery => delivery.EmergencyAlertId == alertId)
            .Select(delivery => delivery.NextOfKinContactId)
            .ToListAsync())
            .ToHashSet();

        return persistedContactIds.Count == expectedContactIds.Count
            && expectedContactIds.All(persistedContactIds.Contains);
    }

    private static async Task<EmergencyAlertDto> TriggerSystemAlertAsync(
        PatientRegistrationTestHost host,
        Guid patientId) =>
        await host.Sender.Send(
            new DispatchSystemEmergencyAlertCommand(patientId, "Critical biometric reading detected."),
            CancellationToken.None);

    private static async Task<EmergencyAlertDto> TriggerDoctorAlertAsync(
        PatientRegistrationTestHost host,
        FakeTimeProvider clock,
        Patient patient)
    {
        var (appointmentId, doctor) = await SeedConfirmedConsultationAsync(host, clock, patient);
        host.CurrentUser.UserId = doctor.UserId;

        return await host.Sender.Send(
            new SendDoctorEmergencyAlertCommand(
                patient.Id,
                appointmentId,
                "Patient collapsed during consultation."),
            CancellationToken.None);
    }

    private static async Task<Patient> SeedPatientAsync(PatientRegistrationTestHost host)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Emergency Alert Property Patient",
                null,
                $"emergency-alert-property-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }

    private static async Task SeedNextOfKinAsync(
        PatientRegistrationTestHost host,
        Guid patientId,
        int contactCount)
    {
        var repository = host.GetRequiredService<INextOfKinRepository>();

        for (var index = 0; index < contactCount; index++)
        {
            await repository.AddAsync(
                NextOfKinContact.Create(
                    patientId,
                    $"Contact {index + 1}",
                    "Sibling",
                    $"+1555100{index:0000}",
                    $"emergency-contact-{index}-{Guid.NewGuid():N}@example.com",
                    index % 2 == 0),
                CancellationToken.None);
        }
    }

    private static async Task<(Guid AppointmentId, Doctor Doctor)> SeedConfirmedConsultationAsync(
        PatientRegistrationTestHost host,
        FakeTimeProvider clock,
        Patient patient)
    {
        var doctorRegistration = await host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await host.Sender.Send(new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId), CancellationToken.None);

        var doctor = await host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        host.CurrentUser.UserId = patient.UserId;
        var booking = await host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                clock.UtcNow.AddDays(1)),
            CancellationToken.None);

        await new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            host.GetRequiredService<IAppointmentRepository>(),
            host.GetRequiredService<IOutboxRepository>(),
            host.GetRequiredService<IDomainEventPublisher>())
            .Handle(
                new PaymentCompletedNotification(booking.AppointmentId, Guid.CreateVersion7(), clock.UtcNow),
                CancellationToken.None);

        return (booking.AppointmentId, doctor);
    }
}
