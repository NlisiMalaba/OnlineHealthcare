using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Telemedicine.Realtime.Files;

public sealed record ShareTelemedicineSessionFileCommand(
    Guid AppointmentId,
    Stream Content,
    string ContentType,
    string FileName,
    long ContentLength) : ICommand<TelemedicineFileSharedDto>;
