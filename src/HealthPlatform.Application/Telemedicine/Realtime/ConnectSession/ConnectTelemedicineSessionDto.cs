namespace HealthPlatform.Application.Telemedicine.Realtime.ConnectSession;

public sealed record ConnectTelemedicineSessionDto(
    Guid AppointmentId,
    string GroupName);
