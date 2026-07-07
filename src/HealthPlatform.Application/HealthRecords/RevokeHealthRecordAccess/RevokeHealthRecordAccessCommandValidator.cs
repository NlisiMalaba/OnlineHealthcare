using FluentValidation;

namespace HealthPlatform.Application.HealthRecords.RevokeHealthRecordAccess;

public sealed class RevokeHealthRecordAccessCommandValidator : AbstractValidator<RevokeHealthRecordAccessCommand>
{
    public RevokeHealthRecordAccessCommandValidator()
    {
        RuleFor(command => command.DoctorId)
            .NotEmpty();
    }
}
