using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Queue.Manage;

public sealed record AdvanceQueueCommand : ICommand<IReadOnlyList<QueueEntryDto>>;
