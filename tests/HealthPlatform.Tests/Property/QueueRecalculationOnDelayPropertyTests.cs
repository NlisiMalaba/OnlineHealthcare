using FsCheck.Xunit;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Queue;
using HealthPlatform.Application.Queue.JoinQueue;
using HealthPlatform.Application.Queue.Manage;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class QueueRecalculationOnDelayPropertyTests
{
    // Feature: online-healthcare-platform, Property 41: Queue Recalculation on Delay
    [Property(Arbitrary = [typeof(QueueArbitraries)], MaxTest = 100)]
    public bool Delay_greater_than_15_minutes_recalculates_all_wait_times_and_notifies_all_patients(
        QueueDelayRecalculationCase input) =>
        RunRecalculationInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunRecalculationInvariantAsync(QueueDelayRecalculationCase input)
    {
        await using var host = new PatientRegistrationTestHost();
        var doctor = await SeedDoctorAsync(host, input.SlotDurationMinutes);

        for (var index = 0; index < input.ExistingPatientsCount; index++)
        {
            var patient = await SeedPatientAsync(host, $"existing-{index}");
            var appointment = await SeedConfirmedAppointmentAsync(host, doctor, patient);
            host.CurrentUser.UserId = patient.UserId;
            await host.Sender.Send(new JoinQueueCommand(appointment.Id), CancellationToken.None);
        }

        host.CurrentUser.UserId = doctor.UserId;
        var recalculated = await host.Sender.Send(
            new RecalculateQueueOnDelayCommand(input.DelayMinutes),
            CancellationToken.None);

        if (recalculated.Count != input.ExistingPatientsCount)
        {
            return false;
        }

        if (host.QueueDelayNotifier.Calls.Count != input.ExistingPatientsCount)
        {
            return false;
        }

        var expectedCallsByQueueEntry = host.QueueDelayNotifier.Calls.ToDictionary(call => call.QueueEntryId);
        if (expectedCallsByQueueEntry.Count != input.ExistingPatientsCount)
        {
            return false;
        }

        for (var index = 0; index < recalculated.Count; index++)
        {
            var dto = recalculated[index];
            var expectedEstimatedWait = input.DelayMinutes + (index * input.SlotDurationMinutes);
            if (dto.EstimatedWaitMinutes != expectedEstimatedWait)
            {
                return false;
            }

            if (!expectedCallsByQueueEntry.TryGetValue(dto.Id, out var call))
            {
                return false;
            }

            if (call.UpdatedEstimatedWaitMinutes != expectedEstimatedWait || call.DelayMinutes != input.DelayMinutes)
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<Doctor> SeedDoctorAsync(PatientRegistrationTestHost host, int slotDurationMinutes)
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
                        slotDurationMinutes,
                        DoctorAppointmentType.Both)
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
                $"Queue Delay Property Patient {suffix}",
                null,
                $"queue-delay-property-{suffix}-{Guid.NewGuid():N}@example.com",
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
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddMinutes(10));
        appointment.ConfirmOnPayment(DateTime.UtcNow);
        host.DbContext.Appointments.Add(appointment);
        await host.DbContext.SaveChangesAsync();
        return appointment;
    }
}
