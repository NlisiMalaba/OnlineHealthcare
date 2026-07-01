using HealthPlatform.Application.NextOfKin;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingNextOfKinDesignationNotifier : INextOfKinDesignationNotifier
{
    public List<DesignationNotificationCall> Calls { get; } = [];

    public Task NotifyDesignatedAsync(
        NextOfKinContactDto contact,
        string patientFullName,
        CancellationToken ct)
    {
        Calls.Add(new DesignationNotificationCall(contact.Id, contact.PatientId, patientFullName));
        return Task.CompletedTask;
    }

    public sealed record DesignationNotificationCall(
        Guid ContactId,
        Guid PatientId,
        string PatientFullName);
}
