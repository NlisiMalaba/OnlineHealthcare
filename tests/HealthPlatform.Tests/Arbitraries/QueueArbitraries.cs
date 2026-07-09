using FsCheck;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record QueueJoinCase(
    int ExistingPatientsCount,
    int SlotDurationMinutes,
    DoctorAppointmentType AppointmentType);

public sealed record QueueDelayRecalculationCase(
    int ExistingPatientsCount,
    int SlotDurationMinutes,
    int DelayMinutes);

public static class QueueArbitraries
{
    public static Arbitrary<QueueJoinCase> QueueJoinCase() =>
        (from existingPatients in Gen.Choose(0, 15)
         from slotDuration in Gen.Elements(15, 20, 30, 45, 60)
         from appointmentType in Gen.Elements(DoctorAppointmentType.Physical, DoctorAppointmentType.Both)
         select new QueueJoinCase(existingPatients, slotDuration, appointmentType))
        .ToArbitrary();

    public static Arbitrary<QueueDelayRecalculationCase> QueueDelayRecalculationCase() =>
        (from existingPatients in Gen.Choose(1, 12)
         from slotDuration in Gen.Elements(15, 20, 30, 45, 60)
         from delayMinutes in Gen.Choose(16, 120)
         select new QueueDelayRecalculationCase(existingPatients, slotDuration, delayMinutes))
        .ToArbitrary();
}
