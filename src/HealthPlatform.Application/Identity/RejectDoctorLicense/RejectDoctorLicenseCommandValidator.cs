using FluentValidation;

namespace HealthPlatform.Application.Identity.RejectDoctorLicense;

public sealed class RejectDoctorLicenseCommandValidator : AbstractValidator<RejectDoctorLicenseCommand>
{
    public RejectDoctorLicenseCommandValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
