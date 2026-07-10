using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Domain.MentalHealth;
using HealthPlatform.Domain.NextOfKin;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.MentalHealth.CrisisProtocol;

public sealed class CrisisProtocolService(
    ICrisisKeywordDetector crisisKeywordDetector,
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    INextOfKinRepository nextOfKinRepository,
    INextOfKinEmergencyAlertDeliveryCoordinator deliveryCoordinator,
    IEmergencyAlertRepository emergencyAlertRepository,
    ILogger<CrisisProtocolService> logger) : ICrisisProtocolService
{
    public async Task<CrisisProtocolDto> TryTriggerAsync(
        Guid patientId,
        string? inputText,
        CrisisProtocolInputSource inputSource,
        CancellationToken ct)
    {
        if (!crisisKeywordDetector.ContainsCrisisKeyword(inputText))
        {
            return CrisisProtocolDto.NotTriggered();
        }

        var patient = await patientRepository.GetByIdAsync(patientId, ct)
            ?? throw new NotFoundException(
                CrisisProtocolErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        var triggeredAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var alert = EmergencyAlert.Create(
            patientId,
            EmergencyAlertTriggerSource.MentalHealthCrisis,
            BuildTriggerReason(inputSource),
            triggeredAtUtc);

        var mentalHealthContacts = (await nextOfKinRepository.ListByPatientIdAsync(patientId, ct))
            .Where(contact => contact.IsMentalHealthContact)
            .Select(contact => contact.ToDto())
            .ToList();

        if (mentalHealthContacts.Count == 0)
        {
            alert.MarkNoContactsAvailable();
            await emergencyAlertRepository.AddAsync(alert, ct);
            await emergencyAlertRepository.SaveChangesAsync(ct);

            logger.LogWarning(
                "Crisis protocol triggered for patient {PatientId} with no mental health next-of-kin contacts.",
                patientId);

            return BuildTriggeredResponse(mentalHealthContactsNotified: 0);
        }

        var contactDeliveries = await deliveryCoordinator.DispatchAsync(
            alert,
            patient,
            mentalHealthContacts,
            ct);

        alert.RecordContactDeliveries(contactDeliveries);
        await emergencyAlertRepository.AddAsync(alert, ct);
        await emergencyAlertRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Crisis protocol alert {AlertId} dispatched to {ContactCount} mental health contact(s) for patient {PatientId}.",
            alert.Id,
            mentalHealthContacts.Count,
            patientId);

        return BuildTriggeredResponse(mentalHealthContacts.Count);
    }

    private static CrisisProtocolDto BuildTriggeredResponse(int mentalHealthContactsNotified) =>
        new(
            Triggered: true,
            EmergencyServicesPrompt: CrisisProtocolPolicies.EmergencyServicesPrompt,
            Helplines: CrisisProtocolPolicies.DefaultHelplines
                .Select(helpline => new CrisisHelplineDto(
                    helpline.Name,
                    helpline.PhoneNumber,
                    helpline.WebsiteUrl))
                .ToList(),
            MentalHealthContactsNotified: mentalHealthContactsNotified);

    private static string BuildTriggerReason(CrisisProtocolInputSource inputSource) =>
        inputSource switch
        {
            CrisisProtocolInputSource.MoodLog =>
                "Crisis keywords detected in mood log notes.",
            CrisisProtocolInputSource.AiAssistant =>
                "Crisis keywords detected in AI assistant input.",
            _ => "Crisis keywords detected in patient input."
        };
}
