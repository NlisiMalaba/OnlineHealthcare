using FluentValidation;

namespace HealthPlatform.Application.Telemedicine.Realtime.Files;

public sealed class ShareTelemedicineSessionFileCommandValidator : AbstractValidator<ShareTelemedicineSessionFileCommand>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public ShareTelemedicineSessionFileCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(contentType => AllowedContentTypes.Contains(contentType))
            .WithErrorCode(TelemedicineErrorCodes.UnsupportedFileType);

        RuleFor(x => x.ContentLength)
            .GreaterThan(0)
            .LessThanOrEqualTo(TelemedicinePolicies.MaxSharedFileBytes);

        RuleFor(x => x.Content)
            .NotNull();
    }
}
