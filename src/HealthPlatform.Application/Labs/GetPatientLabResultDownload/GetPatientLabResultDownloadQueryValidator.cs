using FluentValidation;

namespace HealthPlatform.Application.Labs.GetPatientLabResultDownload;

public sealed class GetPatientLabResultDownloadQueryValidator : AbstractValidator<GetPatientLabResultDownloadQuery>
{
    public GetPatientLabResultDownloadQueryValidator()
    {
        RuleFor(x => x.LabResultId).NotEmpty();
    }
}
