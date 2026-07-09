using FluentValidation;

namespace HealthPlatform.Application.Queue.Manage;

public sealed class MarkQueueEntryAbsentCommandValidator : AbstractValidator<MarkQueueEntryAbsentCommand>
{
    public MarkQueueEntryAbsentCommandValidator()
    {
        RuleFor(x => x.QueueEntryId).NotEmpty();
    }
}
