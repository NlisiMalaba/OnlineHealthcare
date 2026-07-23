using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Wellness;

public sealed class CarePlan : Entity
{
    private CarePlan()
    {
        Condition = string.Empty;
        Tasks = [];
        MonitoringTargets = [];
    }

    public Guid PatientId { get; private set; }

    public Guid DoctorId { get; private set; }

    public string Condition { get; private set; }

    public IReadOnlyList<CarePlanTask> Tasks { get; private set; }

    public IReadOnlyList<CarePlanMonitoringTarget> MonitoringTargets { get; private set; }

    public int ReviewIntervalDays { get; private set; }

    public DateOnly NextReviewAt { get; private set; }

    public DateTime? ReviewReminderSentAtUtc { get; private set; }

    public CarePlanStatus Status { get; private set; }

    public CarePlanProgress Progress => CarePlanProgressCalculator.Calculate(Tasks);

    public static CarePlan Assign(
        Guid patientId,
        Guid doctorId,
        string condition,
        IReadOnlyList<CarePlanTaskDraft> tasks,
        IReadOnlyList<CarePlanMonitoringTargetDraft> monitoringTargets,
        int reviewIntervalDays,
        DateTime assignedAtUtc)
    {
        ValidateAssignment(patientId, doctorId, condition, tasks, monitoringTargets, reviewIntervalDays, assignedAtUtc);

        var assignedDate = DateOnly.FromDateTime(assignedAtUtc);
        return new CarePlan
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            DoctorId = doctorId,
            Condition = condition.Trim(),
            Tasks = tasks.Select(CreateTaskFromDraft).ToList(),
            MonitoringTargets = monitoringTargets.Select(CreateTargetFromDraft).ToList(),
            ReviewIntervalDays = reviewIntervalDays,
            NextReviewAt = assignedDate.AddDays(reviewIntervalDays),
            Status = CarePlanStatus.Active,
            CreatedAtUtc = assignedAtUtc,
            UpdatedAtUtc = assignedAtUtc
        };
    }

    public void Update(
        string condition,
        IReadOnlyList<CarePlanTaskDraft> tasks,
        IReadOnlyList<CarePlanMonitoringTargetDraft> monitoringTargets,
        int reviewIntervalDays,
        DateOnly? nextReviewAt,
        DateTime updatedAtUtc)
    {
        EnsureActive();
        ValidateContent(condition, tasks, monitoringTargets, reviewIntervalDays);

        if (updatedAtUtc == default || updatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Updated timestamp must be UTC.", nameof(updatedAtUtc));
        }

        Condition = condition.Trim();
        ReviewIntervalDays = reviewIntervalDays;
        NextReviewAt = nextReviewAt ?? DateOnly.FromDateTime(updatedAtUtc).AddDays(reviewIntervalDays);
        ReviewReminderSentAtUtc = null;
        Tasks = tasks.Select(CreateTaskFromDraft).ToList();
        MonitoringTargets = monitoringTargets.Select(CreateTargetFromDraft).ToList();
        Touch();
        UpdatedAtUtc = updatedAtUtc;
    }

    public CarePlanTask CompleteTask(Guid taskId, DateTime completedAtUtc)
    {
        EnsureActive();

        var index = IndexOfTask(taskId);
        if (index < 0)
        {
            throw new InvalidOperationException("Care plan task was not found.");
        }

        var updatedTasks = Tasks.ToList();
        var completed = updatedTasks[index].MarkCompleted(completedAtUtc);
        updatedTasks[index] = completed;
        Tasks = updatedTasks;
        Touch();
        UpdatedAtUtc = completedAtUtc;
        return completed;
    }

    public IReadOnlyList<CarePlanTask> ListDueTasksForReminder(DateOnly asOfDate) =>
        Tasks.Where(task => task.IsDueForReminder(asOfDate)).ToList();

    public bool MarkTaskReminderSent(Guid taskId, DateTime sentAtUtc)
    {
        EnsureActive();

        var index = IndexOfTask(taskId);
        if (index < 0)
        {
            return false;
        }

        var existing = Tasks[index];
        if (existing.IsCompleted || existing.ReminderSentAtUtc.HasValue)
        {
            return false;
        }

        var updatedTasks = Tasks.ToList();
        updatedTasks[index] = existing.MarkReminderSent(sentAtUtc);
        Tasks = updatedTasks;
        Touch();
        UpdatedAtUtc = sentAtUtc;
        return true;
    }

    public bool IsDueForReviewReminder(DateOnly asOfDate) =>
        Status == CarePlanStatus.Active
        && ReviewReminderSentAtUtc is null
        && NextReviewAt <= asOfDate;

    public bool MarkReviewReminderSent(DateTime sentAtUtc)
    {
        if (!IsDueForReviewReminder(DateOnly.FromDateTime(sentAtUtc)))
        {
            return false;
        }

        if (sentAtUtc == default || sentAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Sent timestamp must be UTC.", nameof(sentAtUtc));
        }

        ReviewReminderSentAtUtc = sentAtUtc;
        Touch();
        UpdatedAtUtc = sentAtUtc;
        return true;
    }

    public void Complete(DateTime completedAtUtc)
    {
        EnsureActive();
        if (completedAtUtc == default || completedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Completion timestamp must be UTC.", nameof(completedAtUtc));
        }

        Status = CarePlanStatus.Completed;
        Touch();
        UpdatedAtUtc = completedAtUtc;
    }

    public void Archive(DateTime archivedAtUtc)
    {
        if (Status == CarePlanStatus.Archived)
        {
            return;
        }

        if (archivedAtUtc == default || archivedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Archive timestamp must be UTC.", nameof(archivedAtUtc));
        }

        Status = CarePlanStatus.Archived;
        Touch();
        UpdatedAtUtc = archivedAtUtc;
    }

    private int IndexOfTask(Guid taskId)
    {
        for (var i = 0; i < Tasks.Count; i++)
        {
            if (Tasks[i].Id == taskId)
            {
                return i;
            }
        }

        return -1;
    }

    private void EnsureActive()
    {
        if (Status != CarePlanStatus.Active)
        {
            throw new InvalidOperationException("Care plan is not active.");
        }
    }

    private static void ValidateAssignment(
        Guid patientId,
        Guid doctorId,
        string condition,
        IReadOnlyList<CarePlanTaskDraft> tasks,
        IReadOnlyList<CarePlanMonitoringTargetDraft> monitoringTargets,
        int reviewIntervalDays,
        DateTime assignedAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        if (assignedAtUtc == default || assignedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Assignment timestamp must be UTC.", nameof(assignedAtUtc));
        }

        ValidateContent(condition, tasks, monitoringTargets, reviewIntervalDays);
    }

    private static void ValidateContent(
        string condition,
        IReadOnlyList<CarePlanTaskDraft> tasks,
        IReadOnlyList<CarePlanMonitoringTargetDraft> monitoringTargets,
        int reviewIntervalDays)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException("Condition is required.", nameof(condition));
        }

        if (condition.Trim().Length > CarePlanDomainPolicies.MaxConditionLength)
        {
            throw new ArgumentException(
                $"Condition must be at most {CarePlanDomainPolicies.MaxConditionLength} characters.",
                nameof(condition));
        }

        if (tasks is null || tasks.Count == 0)
        {
            throw new ArgumentException("At least one care plan task is required.", nameof(tasks));
        }

        if (tasks.Count > CarePlanDomainPolicies.MaxTasks)
        {
            throw new ArgumentException(
                $"Care plan cannot exceed {CarePlanDomainPolicies.MaxTasks} tasks.",
                nameof(tasks));
        }

        if (monitoringTargets is null || monitoringTargets.Count == 0)
        {
            throw new ArgumentException(
                "At least one monitoring target is required.",
                nameof(monitoringTargets));
        }

        if (monitoringTargets.Count > CarePlanDomainPolicies.MaxMonitoringTargets)
        {
            throw new ArgumentException(
                $"Care plan cannot exceed {CarePlanDomainPolicies.MaxMonitoringTargets} monitoring targets.",
                nameof(monitoringTargets));
        }

        if (reviewIntervalDays < CarePlanDomainPolicies.MinReviewIntervalDays
            || reviewIntervalDays > CarePlanDomainPolicies.MaxReviewIntervalDays)
        {
            throw new ArgumentException(
                $"Review interval must be between {CarePlanDomainPolicies.MinReviewIntervalDays} and {CarePlanDomainPolicies.MaxReviewIntervalDays} days.",
                nameof(reviewIntervalDays));
        }

        foreach (var task in tasks)
        {
            ValidateTaskDraft(task);
        }

        foreach (var target in monitoringTargets)
        {
            ValidateTargetDraft(target);
        }
    }

    private static void ValidateTaskDraft(CarePlanTaskDraft task)
    {
        if (string.IsNullOrWhiteSpace(task.Title))
        {
            throw new ArgumentException("Task title is required.");
        }

        if (task.Title.Trim().Length > CarePlanDomainPolicies.MaxTaskTitleLength)
        {
            throw new ArgumentException(
                $"Task title must be at most {CarePlanDomainPolicies.MaxTaskTitleLength} characters.");
        }

        if (task.Description is { Length: > CarePlanDomainPolicies.MaxTaskDescriptionLength })
        {
            throw new ArgumentException(
                $"Task description must be at most {CarePlanDomainPolicies.MaxTaskDescriptionLength} characters.");
        }

        if (task.DueDate == default)
        {
            throw new ArgumentException("Task due date is required.");
        }
    }

    private static void ValidateTargetDraft(CarePlanMonitoringTargetDraft target)
    {
        if (string.IsNullOrWhiteSpace(target.MetricName))
        {
            throw new ArgumentException("Monitoring target metric name is required.");
        }

        if (target.MetricName.Trim().Length > CarePlanDomainPolicies.MaxMetricNameLength)
        {
            throw new ArgumentException(
                $"Metric name must be at most {CarePlanDomainPolicies.MaxMetricNameLength} characters.");
        }

        if (target.TargetValue <= 0)
        {
            throw new ArgumentException("Monitoring target value must be positive.");
        }

        if (string.IsNullOrWhiteSpace(target.Unit))
        {
            throw new ArgumentException("Monitoring target unit is required.");
        }

        if (target.Unit.Trim().Length > CarePlanDomainPolicies.MaxUnitLength)
        {
            throw new ArgumentException(
                $"Unit must be at most {CarePlanDomainPolicies.MaxUnitLength} characters.");
        }
    }

    private static CarePlanTask CreateTaskFromDraft(CarePlanTaskDraft draft) =>
        new(
            draft.Id == Guid.Empty ? Guid.CreateVersion7() : draft.Id,
            draft.Title.Trim(),
            string.IsNullOrWhiteSpace(draft.Description) ? null : draft.Description.Trim(),
            draft.DueDate,
            draft.CompletedAtUtc,
            draft.ReminderSentAtUtc);

    private static CarePlanMonitoringTarget CreateTargetFromDraft(CarePlanMonitoringTargetDraft draft) =>
        new(draft.MetricName.Trim(), draft.TargetValue, draft.Unit.Trim());
}

public sealed record CarePlanTaskDraft(
    Guid Id,
    string Title,
    string? Description,
    DateOnly DueDate,
    DateTime? CompletedAtUtc = null,
    DateTime? ReminderSentAtUtc = null);

public sealed record CarePlanMonitoringTargetDraft(
    string MetricName,
    decimal TargetValue,
    string Unit);

public static class CarePlanDomainPolicies
{
    public const int MaxConditionLength = 200;

    public const int MaxTaskTitleLength = 200;

    public const int MaxTaskDescriptionLength = 1000;

    public const int MaxMetricNameLength = 100;

    public const int MaxUnitLength = 32;

    public const int MaxTasks = 50;

    public const int MaxMonitoringTargets = 20;

    public const int MinReviewIntervalDays = 1;

    public const int MaxReviewIntervalDays = 365;
}
