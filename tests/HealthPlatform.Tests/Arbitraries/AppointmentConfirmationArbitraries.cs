using FsCheck;
using HealthPlatform.Tests.Arbitraries;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record AppointmentConfirmationFlowCase(
    ValidDoctorRegistration DoctorRegistration,
    int DaysUntilAppointment);

public static class AppointmentConfirmationArbitraries
{
    public static Arbitrary<AppointmentConfirmationFlowCase> AppointmentConfirmationFlowCase() =>
        (from doctor in DoctorRegistrationArbitraries.ValidDoctorRegistration().Generator
         from daysUntilAppointment in Gen.Choose(1, 30)
         select new AppointmentConfirmationFlowCase(doctor, daysUntilAppointment))
        .ToArbitrary();
}
