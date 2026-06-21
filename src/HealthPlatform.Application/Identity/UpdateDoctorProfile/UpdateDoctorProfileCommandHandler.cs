using MediatR;

namespace HealthPlatform.Application.Identity.UpdateDoctorProfile;

public sealed class UpdateDoctorProfileCommandHandler(IDoctorProfileUpdateWorkflow workflow)
    : IRequestHandler<UpdateDoctorProfileCommand, DoctorProfileDto>
{
    public Task<DoctorProfileDto> Handle(UpdateDoctorProfileCommand request, CancellationToken ct) =>
        workflow.UpdateAsync(request, ct);
}
