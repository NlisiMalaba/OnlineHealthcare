using FluentValidation;

namespace HealthPlatform.Application.MentalHealth.CrisisProtocol.EvaluateCrisisInput;

public sealed class EvaluateCrisisInputCommandValidator : AbstractValidator<EvaluateCrisisInputCommand>
{
    private const int MaxInputLength = 4000;

    public EvaluateCrisisInputCommandValidator()
    {
        RuleFor(command => command.InputText)
            .NotEmpty()
            .WithErrorCode(CrisisProtocolErrorCodes.InputTextRequired)
            .MaximumLength(MaxInputLength)
            .WithErrorCode(CrisisProtocolErrorCodes.InputTextTooLong);
    }
}
