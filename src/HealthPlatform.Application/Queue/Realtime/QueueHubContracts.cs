namespace HealthPlatform.Application.Queue.Realtime;

public static class QueueHubEvents
{
    public const string PositionUpdated = "positionUpdated";
}

public static class QueueGroupNames
{
    public static string ForQueueEntry(Guid queueEntryId) => $"queue-entry:{queueEntryId:N}";
}
