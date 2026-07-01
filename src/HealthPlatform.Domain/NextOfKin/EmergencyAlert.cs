using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.NextOfKin;

public sealed class EmergencyAlert : Entity
{
    private readonly List<EmergencyAlertContactDelivery> _contactDeliveries = [];

    private EmergencyAlert()
    {
        TriggerReason = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public EmergencyAlertTriggerSource TriggerSource { get; private set; }

    public Guid? TriggeredByDoctorId { get; private set; }

    public Guid? AppointmentId { get; private set; }

    public string TriggerReason { get; private set; }

    public DateTime TriggeredAtUtc { get; private set; }

    public EmergencyAlertOverallStatus OverallStatus { get; private set; }

    public IReadOnlyCollection<EmergencyAlertContactDelivery> ContactDeliveries => _contactDeliveries;

    public static EmergencyAlert Create(
        Guid patientId,
        EmergencyAlertTriggerSource triggerSource,
        string triggerReason,
        DateTime triggeredAtUtc,
        Guid? triggeredByDoctorId = null,
        Guid? appointmentId = null)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (string.IsNullOrWhiteSpace(triggerReason))
        {
            throw new ArgumentException("Trigger reason is required.", nameof(triggerReason));
        }

        if (triggeredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Triggered time must be UTC.", nameof(triggeredAtUtc));
        }

        if (triggerSource == EmergencyAlertTriggerSource.Doctor && !triggeredByDoctorId.HasValue)
        {
            throw new ArgumentException("Doctor-triggered alerts require a doctor id.", nameof(triggeredByDoctorId));
        }

        return new EmergencyAlert
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            TriggerSource = triggerSource,
            TriggeredByDoctorId = triggeredByDoctorId,
            AppointmentId = appointmentId,
            TriggerReason = triggerReason.Trim(),
            TriggeredAtUtc = triggeredAtUtc,
            OverallStatus = EmergencyAlertOverallStatus.NoContacts
        };
    }

    public void MarkNoContactsAvailable()
    {
        OverallStatus = EmergencyAlertOverallStatus.NoContacts;
        Touch();
    }

    public void RecordContactDeliveries(IEnumerable<EmergencyAlertContactDelivery> deliveries)
    {
        ArgumentNullException.ThrowIfNull(deliveries);
        _contactDeliveries.Clear();
        _contactDeliveries.AddRange(deliveries);
        OverallStatus = ComputeOverallStatus(_contactDeliveries);
        Touch();
    }

    private static EmergencyAlertOverallStatus ComputeOverallStatus(
        IReadOnlyCollection<EmergencyAlertContactDelivery> deliveries)
    {
        if (deliveries.Count == 0)
        {
            return EmergencyAlertOverallStatus.NoContacts;
        }

        var allSucceeded = deliveries.All(delivery =>
            delivery.SmsStatus == EmergencyAlertChannelDeliveryStatus.Sent
            && delivery.PushStatus == EmergencyAlertChannelDeliveryStatus.Sent);

        if (allSucceeded)
        {
            return EmergencyAlertOverallStatus.Dispatched;
        }

        var allFailed = deliveries.All(delivery =>
            delivery.SmsStatus == EmergencyAlertChannelDeliveryStatus.Failed
            && delivery.PushStatus == EmergencyAlertChannelDeliveryStatus.Failed);

        return allFailed
            ? EmergencyAlertOverallStatus.Failed
            : EmergencyAlertOverallStatus.PartiallyFailed;
    }
}
