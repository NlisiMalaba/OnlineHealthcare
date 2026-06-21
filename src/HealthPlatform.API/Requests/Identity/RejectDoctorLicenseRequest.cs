namespace HealthPlatform.API.Requests.Identity;

public sealed class RejectDoctorLicenseRequest
{
    public required string Reason { get; init; }
}
