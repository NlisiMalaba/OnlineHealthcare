using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Queue.Manage;

public sealed record RecalculateQueueOnDelayCommand(int DelayMinutes) : ICommand<IReadOnlyList<QueueEntryDto>>;
