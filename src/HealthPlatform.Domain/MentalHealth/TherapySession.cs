namespace HealthPlatform.Domain.MentalHealth;

public sealed class TherapySession : Common.Entity
{
    private TherapySession()
    {
    }

    public Guid AppointmentId { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid TherapistId { get; private set; }

    public string? SummaryRef { get; private set; }

    public string? SummaryEntryId { get; private set; }

    public bool IsVisibleToPatient { get; private set; }

    public bool BroaderAccessGranted { get; private set; }

    public TherapySessionStatus Status { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public static TherapySession CreateScheduled(
        Guid appointmentId,
        Guid patientId,
        Guid therapistId)
    {
        if (appointmentId == Guid.Empty)
        {
            throw new ArgumentException("Appointment id is required.", nameof(appointmentId));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (therapistId == Guid.Empty)
        {
            throw new ArgumentException("Therapist id is required.", nameof(therapistId));
        }

        return new TherapySession
        {
            Id = Guid.CreateVersion7(),
            AppointmentId = appointmentId,
            PatientId = patientId,
            TherapistId = therapistId,
            IsVisibleToPatient = true,
            BroaderAccessGranted = false,
            Status = TherapySessionStatus.Scheduled
        };
    }

    public void Complete(string summaryRef, string summaryEntryId, DateTime completedAtUtc)
    {
        if (Status != TherapySessionStatus.Scheduled)
        {
            throw new TherapySessionCompletionNotAllowedException(Status);
        }

        if (string.IsNullOrWhiteSpace(summaryRef))
        {
            throw new ArgumentException("Summary reference is required.", nameof(summaryRef));
        }

        if (string.IsNullOrWhiteSpace(summaryEntryId))
        {
            throw new ArgumentException("Summary entry id is required.", nameof(summaryEntryId));
        }

        if (completedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Completion time must be UTC.", nameof(completedAtUtc));
        }

        SummaryRef = summaryRef.Trim();
        SummaryEntryId = summaryEntryId.Trim();
        Status = TherapySessionStatus.Completed;
        CompletedAtUtc = completedAtUtc;
        Touch();
    }

    public void GrantBroaderAccess(DateTime grantedAtUtc)
    {
        if (Status != TherapySessionStatus.Completed)
        {
            throw new TherapySessionBroaderAccessNotAllowedException(Status);
        }

        if (grantedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Grant time must be UTC.", nameof(grantedAtUtc));
        }

        if (BroaderAccessGranted)
        {
            return;
        }

        BroaderAccessGranted = true;
        Touch();
    }
}
