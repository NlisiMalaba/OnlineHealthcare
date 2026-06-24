using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.Application.Telemedicine;

public sealed record JoinTelemedicineSessionDto(
    Guid SessionId,
    Guid AppointmentId,
    string ChannelName,
    string RtcToken,
    RtcProvider RtcProvider,
    uint Uid,
    TelemedicineSessionMode Mode,
    DateTime ExpiresAtUtc);
