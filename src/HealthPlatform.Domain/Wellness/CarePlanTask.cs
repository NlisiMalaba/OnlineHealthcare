namespace HealthPlatform.Domain.Wellness;

public sealed record CarePlanTask(
    Guid Id,
    string Title,
    string? Description,
    DateOnly DueDate,
    DateTime? CompletedAtUtc,
    DateTime? ReminderSentAtUtc)
{
    public bool IsCompleted => CompletedAtUtc.HasValue;

    public bool IsDueForReminder(DateOnly asOfDate) =>
        !IsCompleted
        && ReminderSentAtUtc is null
        && DueDate <= asOfDate;

    public CarePlanTask MarkCompleted(DateTime completedAtUtc)
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException("Care plan task is already completed.");
        }

        if (completedAtUtc == default || completedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Completion timestamp must be UTC.", nameof(completedAtUtc));
        }

        return this with { CompletedAtUtc = completedAtUtc };
    }

    public CarePlanTask MarkReminderSent(DateTime sentAtUtc)
    {
        if (IsCompleted || ReminderSentAtUtc.HasValue)
        {
            return this;
        }

        if (sentAtUtc == default || sentAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Sent timestamp must be UTC.", nameof(sentAtUtc));
        }

        return this with { ReminderSentAtUtc = sentAtUtc };
    }
}
