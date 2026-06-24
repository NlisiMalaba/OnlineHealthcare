using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Telemedicine;

public sealed class TelemedicineSession : Entity
{
    private TelemedicineSession()
    {
        ChannelName = string.Empty;
    }

    public Guid AppointmentId { get; private set; }

    public string ChannelName { get; private set; }

    public RtcProvider RtcProvider { get; private set; }

    public TelemedicineSessionMode Mode { get; private set; }

    public TelemedicineSessionStatus Status { get; private set; }

    public bool RecordingConsent { get; private set; }

    public bool RecordingEnabled { get; private set; }

    public string? RecordingUrl { get; private set; }

    public DateTime? StartedAtUtc { get; private set; }

    public DateTime? EndedAtUtc { get; private set; }

    public DateTime? InterruptedAtUtc { get; private set; }

    public int DurationSeconds { get; private set; }

    public string? SessionSummaryRef { get; private set; }

    public static TelemedicineSession CreateForAppointment(Guid appointmentId, RtcProvider rtcProvider)
    {
        if (appointmentId == Guid.Empty)
        {
            throw new ArgumentException("Appointment id is required.", nameof(appointmentId));
        }

        return new TelemedicineSession
        {
            Id = Guid.CreateVersion7(),
            AppointmentId = appointmentId,
            ChannelName = BuildChannelName(appointmentId),
            RtcProvider = rtcProvider,
            Mode = TelemedicineSessionMode.Video,
            Status = TelemedicineSessionStatus.Waiting,
            RecordingConsent = false,
            RecordingEnabled = false
        };
    }

    public void MarkJoined(DateTime joinedAtUtc, TelemedicineSessionMode? requestedMode)
    {
        if (joinedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Join time must be UTC.", nameof(joinedAtUtc));
        }

        if (Status is TelemedicineSessionStatus.Ended or TelemedicineSessionStatus.Interrupted)
        {
            throw new TelemedicineSessionNotJoinableException(Status);
        }

        if (requestedMode.HasValue)
        {
            Mode = requestedMode.Value;
        }

        if (Status == TelemedicineSessionStatus.Waiting)
        {
            Status = TelemedicineSessionStatus.Active;
            StartedAtUtc = joinedAtUtc;
        }

        Touch();
    }

    public void GrantRecordingConsent(DateTime consentedAtUtc)
    {
        if (consentedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Consent time must be UTC.", nameof(consentedAtUtc));
        }

        if (Status != TelemedicineSessionStatus.Waiting)
        {
            throw new RecordingConsentNotAllowedException(Status);
        }

        if (RecordingConsent)
        {
            return;
        }

        RecordingConsent = true;
        Touch();
    }

    public void EnableRecording()
    {
        if (!RecordingConsent)
        {
            throw new RecordingConsentRequiredException();
        }

        if (Status is TelemedicineSessionStatus.Ended or TelemedicineSessionStatus.Interrupted)
        {
            throw new TelemedicineSessionNotJoinableException(Status);
        }

        if (RecordingEnabled)
        {
            return;
        }

        RecordingEnabled = true;
        Touch();
    }

    public void DisableRecording()
    {
        if (!RecordingEnabled)
        {
            return;
        }

        RecordingEnabled = false;
        Touch();
    }

    public bool BeginReconnectionGrace(DateTime interruptedAtUtc)
    {
        if (interruptedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Interruption time must be UTC.", nameof(interruptedAtUtc));
        }

        if (Status != TelemedicineSessionStatus.Active)
        {
            return false;
        }

        if (InterruptedAtUtc.HasValue)
        {
            return false;
        }

        InterruptedAtUtc = interruptedAtUtc;
        Touch();
        return true;
    }

    public bool TryCompleteReconnection(DateTime reconnectedAtUtc, TimeSpan gracePeriod)
    {
        if (reconnectedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Reconnection time must be UTC.", nameof(reconnectedAtUtc));
        }

        if (!InterruptedAtUtc.HasValue)
        {
            return false;
        }

        if (reconnectedAtUtc > InterruptedAtUtc.Value.Add(gracePeriod))
        {
            throw new TelemedicineReconnectionGraceExpiredException();
        }

        InterruptedAtUtc = null;
        Touch();
        return true;
    }

    public bool ExpireReconnectionGraceIfDue(DateTime asOfUtc, TimeSpan gracePeriod)
    {
        if (asOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Expiration check time must be UTC.", nameof(asOfUtc));
        }

        if (Status != TelemedicineSessionStatus.Active || !InterruptedAtUtc.HasValue)
        {
            return false;
        }

        if (asOfUtc < InterruptedAtUtc.Value.Add(gracePeriod))
        {
            return false;
        }

        Status = TelemedicineSessionStatus.Interrupted;
        Touch();
        return true;
    }

    public DateTime? GetReconnectionDeadlineUtc(TimeSpan gracePeriod) =>
        InterruptedAtUtc?.Add(gracePeriod);

    private static string BuildChannelName(Guid appointmentId) => $"tm-{appointmentId:N}";
}
