using FsCheck.Xunit;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Queue;
using HealthPlatform.Application.Queue.JoinQueue;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class QueuePositionAndWaitTimeAssignmentPropertyTests
{
    // Feature: online-healthcare-platform, Property 40: Queue Position and Wait Time Assignment
    [Property(Arbitrary = [typeof(QueueArbitraries)], MaxTest = 100)]
    public bool Joining_queue_assigns_positive_position_and_non_negative_wait(QueueJoinCase input) =>
        RunQueueAssignmentInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunQueueAssignmentInvariantAsync(QueueJoinCase input)
    {
        await using var host = new PatientRegistrationTestHost();

        var doctor = await SeedDoctorWithPhysicalSlotAsync(host, input);
        var averageConsultationDurationMinutes = QueueConsultationDurationResolver.ResolveAverageMinutes(
            doctor.AvailabilitySlots);

        for (var index = 0; index < input.ExistingPatientsCount; index++)
        {
            var existingPatient = await SeedPatientAsync(host, $"existing-{index}");
            var existingAppointment = await SeedConfirmedAppointmentAsync(host, doctor, existingPatient);
            host.CurrentUser.UserId = existingPatient.UserId;
            await host.Sender.Send(new JoinQueueCommand(existingAppointment.Id), CancellationToken.None);
        }

        var joiningPatient = await SeedPatientAsync(host, "joining");
        var joiningAppointment = await SeedConfirmedAppointmentAsync(host, doctor, joiningPatient);
        host.CurrentUser.UserId = joiningPatient.UserId;

        var result = await host.Sender.Send(new JoinQueueCommand(joiningAppointment.Id), CancellationToken.None);

        if (result.QueuePosition <= 0 || result.EstimatedWaitMinutes < 0)
        {
            return false;
        }

        var expectedPosition = input.ExistingPatientsCount + 1;
        var expectedWaitMinutes = input.ExistingPatientsCount * averageConsultationDurationMinutes;

        return result.QueuePosition == expectedPosition
            && result.EstimatedWaitMinutes == expectedWaitMinutes;
    }

    private static async Task<Doctor> SeedDoctorWithPhysicalSlotAsync(
        PatientRegistrationTestHost host,
        QueueJoinCase input)
    {
        var registration = await host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand() with
            {
                AvailabilitySlots =
                [
                    new DoctorAvailabilitySlotInput(
                        DayOfWeek.Monday,
                        new TimeOnly(8, 0),
                        new TimeOnly(12, 0),
                        input.SlotDurationMinutes,
                        input.AppointmentType)
                ]
            },
            CancellationToken.None);

        await host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);

        return await host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == registration.DoctorId);
    }

    private static async Task<Patient> SeedPatientAsync(PatientRegistrationTestHost host, string suffix)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Queue Property Patient {suffix}",
                null,
                $"queue-property-{suffix}-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }

    private static async Task<Appointment> SeedConfirmedAppointmentAsync(
        PatientRegistrationTestHost host,
        Doctor doctor,
        Patient patient)
    {
        var slot = doctor.AvailabilitySlots.Single();
        var appointment = Appointment.CreatePendingPayment(
            patient.Id,
            doctor.Id,
            slot.Id,
            ConsultationType.General,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddMinutes(10));
        appointment.ConfirmOnPayment(DateTime.UtcNow);

        host.DbContext.Appointments.Add(appointment);
        await host.DbContext.SaveChangesAsync();
        return appointment;
    }
}
