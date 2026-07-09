using FluentValidation;

namespace HealthPlatform.Application.Queue.Manage;

public sealed class MarkQueueEntrySeenCommandValidator : AbstractValidator<MarkQueueEntrySeenCommand>
{
    public MarkQueueEntrySeenCommandValidator()
    {
        RuleFor(x => x.QueueEntryId).NotEmpty();
    }
}
