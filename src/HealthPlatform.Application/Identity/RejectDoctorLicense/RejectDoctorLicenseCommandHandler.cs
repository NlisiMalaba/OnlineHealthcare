using MediatR;

namespace HealthPlatform.Application.Identity.RejectDoctorLicense;

public sealed class RejectDoctorLicenseCommandHandler(ILicenseVerificationWorkflow workflow)
    : IRequestHandler<RejectDoctorLicenseCommand, LicenseVerificationResultDto>
{
    public Task<LicenseVerificationResultDto> Handle(RejectDoctorLicenseCommand request, CancellationToken ct) =>
        workflow.RejectAsync(request.DoctorId, request.Reason, ct);
}
