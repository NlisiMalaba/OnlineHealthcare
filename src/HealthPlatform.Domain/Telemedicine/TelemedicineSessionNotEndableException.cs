namespace HealthPlatform.Domain.Telemedicine;

public sealed class TelemedicineSessionNotEndableException(TelemedicineSessionStatus status)
    : Exception($"Telemedicine session cannot be ended while status is {status}.")
{
    public TelemedicineSessionStatus Status { get; } = status;
}
