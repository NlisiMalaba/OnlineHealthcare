using HealthPlatform.Application.Search;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Search.Documents;

namespace HealthPlatform.Infrastructure.Search;

internal static class DoctorSearchDocumentMapper
{
    public static DoctorSearchDocument Map(Doctor doctor)
    {
        var activeSlots = doctor.AvailabilitySlots
            .Where(slot => slot.IsActive)
            .Select(slot => new DoctorAvailabilitySlotDocument
            {
                DayOfWeek = (int)slot.DayOfWeek,
                StartTime = slot.StartTime.ToString("HH:mm:ss"),
                EndTime = slot.EndTime.ToString("HH:mm:ss"),
                SlotDurationMinutes = slot.SlotDurationMinutes,
                AppointmentType = slot.AppointmentType.ToString().ToLowerInvariant(),
                IsActive = slot.IsActive
            })
            .ToList();

        var minFee = Math.Min(doctor.VirtualFee, doctor.PhysicalFee);
        var maxFee = Math.Max(doctor.VirtualFee, doctor.PhysicalFee);

        return new DoctorSearchDocument
        {
            DoctorId = doctor.Id.ToString(),
            Name = doctor.FullName,
            Specialty = doctor.Specialty,
            AverageRating = (double)doctor.AverageRating,
            TotalReviews = doctor.TotalReviews,
            ClinicLocation = GeoLocationDocument.FromGeoPoint(doctor.ClinicLocation),
            VirtualFee = doctor.VirtualFee,
            PhysicalFee = doctor.PhysicalFee,
            MinFee = minFee,
            MaxFee = maxFee,
            HasAvailability = activeSlots.Count > 0,
            Availability = activeSlots,
            IsSearchable = doctor.VerificationStatus == DoctorVerificationStatus.Verified
        };
    }
}
