namespace HealthPlatform.Domain.Identity;

public sealed class DoctorAvailabilitySlot
{
    private DoctorAvailabilitySlot()
    {
    }

    public Guid Id { get; private set; }

    public Guid DoctorId { get; private set; }

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public int SlotDurationMinutes { get; private set; }

    public DoctorAppointmentType AppointmentType { get; private set; }

    public bool IsActive { get; private set; }

    public static DoctorAvailabilitySlot Create(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int slotDurationMinutes,
        DoctorAppointmentType appointmentType)
    {
        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        if (startTime >= endTime)
        {
            throw new ArgumentException("Start time must be before end time.");
        }

        if (slotDurationMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slotDurationMinutes));
        }

        return new DoctorAvailabilitySlot
        {
            Id = Guid.CreateVersion7(),
            DoctorId = doctorId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            SlotDurationMinutes = slotDurationMinutes,
            AppointmentType = appointmentType,
            IsActive = true
        };
    }
}
