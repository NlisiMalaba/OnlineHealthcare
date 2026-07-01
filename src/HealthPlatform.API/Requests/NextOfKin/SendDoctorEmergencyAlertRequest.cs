namespace HealthPlatform.API.Requests.NextOfKin;

public sealed class SendDoctorEmergencyAlertRequest
{
    public required Guid PatientId { get; init; }

    public required Guid AppointmentId { get; init; }

    public required string TriggerReason { get; init; }
}
