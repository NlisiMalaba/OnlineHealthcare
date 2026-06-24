namespace HealthPlatform.Application.Telemedicine.Realtime;

public sealed record TelemedicineFileSharedDto(
    Guid AppointmentId,
    Guid ShareId,
    string FileName,
    string ContentType,
    string DownloadUrl,
    DateTime SharedAtUtc);
