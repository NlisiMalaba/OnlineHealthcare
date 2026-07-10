namespace HealthPlatform.Domain.Queue;

public enum QueueArrivalStatus
{
    NotArrived = 0,
    Arrived = 1,
    Called = 2,
    Seen = 3,
    Absent = 4
}
