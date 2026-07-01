namespace HealthPlatform.Application.NextOfKin;

public interface INextOfKinDesignationNotifier
{
    Task NotifyDesignatedAsync(
        NextOfKinContactDto contact,
        string patientFullName,
        CancellationToken ct);
}
