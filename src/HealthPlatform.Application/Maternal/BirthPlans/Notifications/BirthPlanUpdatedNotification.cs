using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.Notifications;

public sealed record BirthPlanUpdatedNotification(
    Guid BirthPlanId,
    Guid AntenatalRecordId,
    Guid PatientId,
    Guid ObstetricDoctorId,
    DateTime UpdatedAtUtc,
    DateTime OccurredAtUtc) : INotification;
