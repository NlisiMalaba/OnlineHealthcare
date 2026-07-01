using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.NextOfKin;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.NextOfKin;

public sealed class EmergencyAlertDispatchService(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    INextOfKinRepository nextOfKinRepository,
    INextOfKinEmergencyAlertNotifier emergencyAlertNotifier,
    IEmergencyAlertRepository emergencyAlertRepository,
    ILogger<EmergencyAlertDispatchService> logger) : IEmergencyAlertDispatchService
{
    public async Task<EmergencyAlertDto> DispatchAsync(EmergencyAlertDispatchRequest request, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(request.PatientId, ct)
            ?? throw new Exceptions.NotFoundException(
                NextOfKinErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        var triggeredAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var alert = EmergencyAlert.Create(
            request.PatientId,
            request.TriggerSource,
            request.TriggerReason,
            triggeredAtUtc,
            request.TriggeredByDoctorId,
            request.AppointmentId);

        var contacts = await nextOfKinRepository.ListByPatientIdAsync(request.PatientId, ct);
        if (contacts.Count == 0)
        {
            alert.MarkNoContactsAvailable();
            await emergencyAlertRepository.AddAsync(alert, ct);
            await emergencyAlertRepository.SaveChangesAsync(ct);

            logger.LogWarning(
                "Emergency alert {AlertId} for patient {PatientId} has no next-of-kin contacts to notify.",
                alert.Id,
                request.PatientId);
            return alert.ToDto();
        }

        var contactDtos = contacts.Select(contact => contact.ToDto()).ToList();
        var deliveryResults = await emergencyAlertNotifier.NotifyAllContactsAsync(
            alert.Id,
            patient.Id,
            patient.FullName,
            alert.TriggerReason,
            contactDtos,
            ct);

        var contactDeliveries = deliveryResults
            .Select(result => EmergencyAlertContactDelivery.Create(
                alert.Id,
                result.NextOfKinContactId,
                result.SmsStatus,
                result.PushStatus))
            .ToList();

        alert.RecordContactDeliveries(contactDeliveries);
        await emergencyAlertRepository.AddAsync(alert, ct);
        await emergencyAlertRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Emergency alert {AlertId} dispatched to {ContactCount} next-of-kin contact(s) for patient {PatientId} with status {OverallStatus}.",
            alert.Id,
            contactDtos.Count,
            request.PatientId,
            alert.OverallStatus);

        return alert.ToDto();
    }
}
