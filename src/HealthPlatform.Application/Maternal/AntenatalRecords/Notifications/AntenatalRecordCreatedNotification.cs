using MediatR;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.Notifications;

public sealed record AntenatalRecordCreatedNotification(
    Guid AntenatalRecordId,
    Guid PatientId,
    Guid ObstetricDoctorId,
    DateOnly EstimatedDueDate,
    int GestationalAgeWeeks,
    DateTime CreatedAtUtc,
    DateTime OccurredAtUtc) : INotification;
