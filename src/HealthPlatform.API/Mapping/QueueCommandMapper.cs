using HealthPlatform.API.Requests.Queue;
using HealthPlatform.Application.Queue.JoinQueue;

namespace HealthPlatform.API.Mapping;

public static class QueueCommandMapper
{
    public static JoinQueueCommand ToJoinCommand(JoinQueueRequest request) =>
        new(request.AppointmentId);
}
