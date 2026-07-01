using MediatR;

namespace HealthPlatform.Application.Prescriptions.Notifications;

public sealed record PrescriptionDispensedNotification(
    Guid PrescriptionId,
    Guid PatientId,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    DateTime DispensedAtUtc,
    DateTime OccurredAtUtc) : INotification;
