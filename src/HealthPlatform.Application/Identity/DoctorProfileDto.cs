using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity;

public sealed record DoctorAvailabilitySlotDto(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    DoctorAppointmentType AppointmentType,
    bool IsActive);

public sealed record DoctorProfileDto(
    Guid DoctorId,
    string FullName,
    string Specialty,
    decimal VirtualFee,
    decimal PhysicalFee,
    string? Bio,
    string? ProfilePhotoUrl,
    string VerificationStatus,
    IReadOnlyList<DoctorAvailabilitySlotDto> AvailabilitySlots,
    DateTime UpdatedAtUtc);
