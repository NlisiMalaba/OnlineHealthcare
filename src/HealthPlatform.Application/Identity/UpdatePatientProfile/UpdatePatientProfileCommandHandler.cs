using HealthPlatform.Application.Identity.UpdatePatientProfile;
using MediatR;

namespace HealthPlatform.Application.Identity.UpdatePatientProfile;

public sealed class UpdatePatientProfileCommandHandler(IPatientProfileUpdateWorkflow workflow)
    : IRequestHandler<UpdatePatientProfileCommand, PatientProfileDto>
{
    public Task<PatientProfileDto> Handle(UpdatePatientProfileCommand request, CancellationToken ct) =>
        workflow.UpdateAsync(request, ct);
}
