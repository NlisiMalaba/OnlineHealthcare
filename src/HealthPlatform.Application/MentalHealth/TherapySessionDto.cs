using HealthPlatform.Domain.MentalHealth;

namespace HealthPlatform.Application.MentalHealth;

public sealed record TherapySessionDto(
    Guid Id,
    Guid AppointmentId,
    Guid PatientId,
    Guid TherapistId,
    string? SummaryRef,
    string? SummaryEntryId,
    bool IsVisibleToPatient,
    bool BroaderAccessGranted,
    string Status,
    DateTime? CompletedAtUtc);
