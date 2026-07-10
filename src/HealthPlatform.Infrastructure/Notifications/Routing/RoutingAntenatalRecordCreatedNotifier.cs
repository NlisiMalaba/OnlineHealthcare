using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.Maternal;
using HealthPlatform.Infrastructure.Notifications.Routing;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingAntenatalRecordCreatedNotifier(INotificationDispatcher dispatcher)
    : IAntenatalRecordCreatedNotifier
{
    public async Task NotifyAntenatalRecordCreatedAsync(
        Guid patientUserId,
        Guid obstetricDoctorUserId,
        Guid antenatalRecordId,
        DateOnly estimatedDueDate,
        int recommendedCheckupCount,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["antenatal_record_id"] = antenatalRecordId.ToString(),
            ["estimated_due_date"] = estimatedDueDate.ToString("O"),
            ["recommended_checkup_count"] = recommendedCheckupCount.ToString()
        };

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.AntenatalRecordCreated,
            NotificationCriticality.Standard,
            "Antenatal record created",
            $"Your antenatal care plan has been created with {recommendedCheckupCount} recommended checkups.",
            data,
            ct);

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            obstetricDoctorUserId,
            NotificationRecipientType.Doctor,
            NotificationEventTypes.AntenatalRecordCreated,
            NotificationCriticality.Standard,
            "Patient antenatal record created",
            "A patient has created an antenatal record with you as the assigned obstetric doctor.",
            data,
            ct);
    }
}
