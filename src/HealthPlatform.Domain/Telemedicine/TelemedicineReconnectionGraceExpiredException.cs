namespace HealthPlatform.Domain.Telemedicine;

public sealed class TelemedicineReconnectionGraceExpiredException()
    : Exception("Telemedicine session reconnection grace period has expired.");
