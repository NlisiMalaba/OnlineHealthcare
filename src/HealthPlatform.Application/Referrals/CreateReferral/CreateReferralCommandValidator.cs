using FluentValidation;

namespace HealthPlatform.Application.Referrals.CreateReferral;

public sealed class CreateReferralCommandValidator : AbstractValidator<CreateReferralCommand>
{
    public CreateReferralCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.ClinicalNotes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.ClinicalNotes));

        RuleFor(x => x.ReceivingHospitalName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.ReceivingHospitalName));

        RuleFor(x => x.SharedHealthRecordSections)
            .NotNull()
            .Must(sections => sections.Count > 0)
            .WithMessage("At least one health record section must be shared.");

        RuleForEach(x => x.SharedHealthRecordSections)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PatientConsentAtUtc)
            .Must(consentAtUtc => consentAtUtc != default && consentAtUtc.Kind == DateTimeKind.Utc)
            .WithMessage("Patient consent timestamp must be a UTC value.");

        RuleFor(x => x)
            .Must(x => x.ReceivingDoctorId.HasValue || !string.IsNullOrWhiteSpace(x.ReceivingHospitalName))
            .WithMessage("Either receiving doctor or receiving hospital is required.");
    }
}
