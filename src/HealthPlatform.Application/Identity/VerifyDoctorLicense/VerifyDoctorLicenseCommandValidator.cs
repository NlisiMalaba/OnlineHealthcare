using FluentValidation;

namespace HealthPlatform.Application.Identity.VerifyDoctorLicense;

public sealed class VerifyDoctorLicenseCommandValidator : AbstractValidator<VerifyDoctorLicenseCommand>
{
    public VerifyDoctorLicenseCommandValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty();
    }
}
