namespace HealthPlatform.Application.Appointments.CancelAppointment;

public sealed record CancelAppointmentDto(
    Guid AppointmentId,
    string Status,
    bool IsEarlyCancellation,
    bool SlotReleased,
    bool RefundRequested,
    decimal AppliedLateCancellationRetentionPercent);
