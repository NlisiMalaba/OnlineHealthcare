namespace HealthPlatform.Domain.Telemedicine;

public sealed class TelemedicineSessionNotJoinableException(TelemedicineSessionStatus status)
    : Exception($"Telemedicine session cannot be joined while status is {status}.")
{
    public TelemedicineSessionStatus Status { get; } = status;
}
