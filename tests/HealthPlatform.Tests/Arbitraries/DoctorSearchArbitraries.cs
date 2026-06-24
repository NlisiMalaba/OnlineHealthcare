using FsCheck;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Tests.Generators;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record DoctorProximitySearchCase(
    GeoPoint PatientLocation,
    IReadOnlyList<IndexedDoctorForSearch> Doctors);

public sealed record IndexedDoctorForSearch(
    Guid DoctorId,
    GeoPoint ClinicLocation,
    string Specialty,
    double AverageRating,
    decimal VirtualFee,
    decimal PhysicalFee,
    bool HasAvailability);

public static class DoctorSearchArbitraries
{
    private static readonly string[] Specialties =
    [
        "General Practice",
        "Cardiology",
        "Dermatology",
        "Pediatrics",
        "Orthopedics"
    ];

    public static Arbitrary<DoctorProximitySearchCase> ProximitySearchCase() =>
        (from patientLocation in DomainGenerators.WildGeoPoint().Generator
         from doctorCount in Gen.Choose(2, 12)
         from doctors in Gen.ListOf(doctorCount, IndexedDoctor().Generator)
         select new DoctorProximitySearchCase(patientLocation, doctors.ToList()))
        .ToArbitrary();

    private static Arbitrary<IndexedDoctorForSearch> IndexedDoctor() =>
        (from doctorId in DomainGenerators.NonEmptyGuid().Generator
         from clinicLocation in DomainGenerators.WildGeoPoint().Generator
         from specialty in Gen.Elements(Specialties)
         from averageRating in Gen.Choose(0, 50).Select(value => value / 10.0)
         from virtualFee in Gen.Choose(10, 500).Select(value => (decimal)value)
         from physicalFee in Gen.Choose(10, 500).Select(value => (decimal)value)
         from hasAvailability in Arb.Generate<bool>()
         select new IndexedDoctorForSearch(
             doctorId,
             clinicLocation,
             specialty,
             averageRating,
             virtualFee,
             physicalFee,
             hasAvailability))
        .ToArbitrary();
}
