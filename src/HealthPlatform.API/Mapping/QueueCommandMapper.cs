using HealthPlatform.API.Requests.Queue;
using HealthPlatform.Application.Queue.Manage;
using HealthPlatform.Application.Queue.JoinQueue;

namespace HealthPlatform.API.Mapping;

public static class QueueCommandMapper
{
    public static JoinQueueCommand ToJoinCommand(JoinQueueRequest request) =>
        new(request.AppointmentId);

    public static AdvanceQueueCommand ToAdvanceCommand() => new();

    public static MarkQueueEntrySeenCommand ToMarkSeenCommand(Guid queueEntryId) => new(queueEntryId);

    public static MarkQueueEntryAbsentCommand ToMarkAbsentCommand(Guid queueEntryId) => new(queueEntryId);

    public static RecalculateQueueOnDelayCommand ToRecalculateOnDelayCommand(RecalculateQueueOnDelayRequest request) =>
        new(request.DelayMinutes);
}
