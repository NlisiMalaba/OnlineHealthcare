using MediatR;

namespace HealthPlatform.Application.Identity.VerifyDoctorLicense;

public sealed class VerifyDoctorLicenseCommandHandler(ILicenseVerificationWorkflow workflow)
    : IRequestHandler<VerifyDoctorLicenseCommand, LicenseVerificationResultDto>
{
    public Task<LicenseVerificationResultDto> Handle(VerifyDoctorLicenseCommand request, CancellationToken ct) =>
        workflow.VerifyAsync(request.DoctorId, ct);
}
