using HealthPlatform.Domain.Identity;

namespace HealthPlatform.API.Requests.Appointments;

public sealed class DoctorAvailabilitySlotUpsertRequest
{
    public DayOfWeek DayOfWeek { get; init; }

    public string StartTime { get; init; } = string.Empty;

    public string EndTime { get; init; } = string.Empty;

    public int SlotDurationMinutes { get; init; }

    public DoctorAppointmentType AppointmentType { get; init; }
}
