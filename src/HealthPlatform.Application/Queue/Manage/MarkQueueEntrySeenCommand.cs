using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Queue.Manage;

public sealed record MarkQueueEntrySeenCommand(Guid QueueEntryId) : ICommand<QueueEntryDto>;
