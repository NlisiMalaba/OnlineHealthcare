using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Search;

public sealed record DoctorAvailabilityIndexEntry(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    DoctorAppointmentType AppointmentType,
    bool IsActive);

public interface ISearchService
{
    Task UpdateDoctorAvailabilityIndexAsync(
        Guid doctorId,
        IReadOnlyList<DoctorAvailabilityIndexEntry> availabilitySlots,
        CancellationToken ct);
}
