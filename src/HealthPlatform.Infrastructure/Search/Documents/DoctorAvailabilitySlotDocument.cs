namespace HealthPlatform.Infrastructure.Search.Documents;

public sealed class DoctorAvailabilitySlotDocument
{
    public int DayOfWeek { get; init; }

    public string StartTime { get; init; } = string.Empty;

    public string EndTime { get; init; } = string.Empty;

    public int SlotDurationMinutes { get; init; }

    public string AppointmentType { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
