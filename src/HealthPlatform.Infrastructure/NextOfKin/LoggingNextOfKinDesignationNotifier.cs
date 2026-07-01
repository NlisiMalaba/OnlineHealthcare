using HealthPlatform.Application.NextOfKin;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.NextOfKin;

public sealed class LoggingNextOfKinDesignationNotifier(
    ILogger<LoggingNextOfKinDesignationNotifier> logger) : INextOfKinDesignationNotifier
{
    public Task NotifyDesignatedAsync(
        NextOfKinContactDto contact,
        string patientFullName,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Next-of-kin designation notification requested for contact {ContactId} ({FullName}) designated by patient {PatientId} ({PatientFullName}). Channels: SMS to {PhoneNumber}, email to {Email}. Mental health contact: {IsMentalHealthContact}.",
            contact.Id,
            contact.FullName,
            contact.PatientId,
            patientFullName,
            contact.PhoneNumber,
            contact.Email ?? "(none)",
            contact.IsMentalHealthContact);
        return Task.CompletedTask;
    }
}
