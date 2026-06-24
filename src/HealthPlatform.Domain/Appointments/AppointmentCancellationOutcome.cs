namespace HealthPlatform.Domain.Appointments;

public sealed record AppointmentCancellationOutcome(
    bool IsEarlyCancellation,
    decimal AppliedLateCancellationRetentionPercent);
