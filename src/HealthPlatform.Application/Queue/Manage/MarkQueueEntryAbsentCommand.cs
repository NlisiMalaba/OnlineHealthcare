using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Queue.Manage;

public sealed record MarkQueueEntryAbsentCommand(Guid QueueEntryId) : ICommand;
