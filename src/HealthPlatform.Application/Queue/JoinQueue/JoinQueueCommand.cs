using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Queue.JoinQueue;

public sealed record JoinQueueCommand(Guid AppointmentId) : ICommand<QueueEntryDto>;
