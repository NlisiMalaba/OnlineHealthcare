using MediatR;

namespace HealthPlatform.Application.Auth;

public sealed class CompleteDeviceLoginCommandHandler(IAuthLoginWorkflow workflow)
    : IRequestHandler<CompleteDeviceLoginCommand, LoginResponseDto>
{
    public Task<LoginResponseDto> Handle(CompleteDeviceLoginCommand request, CancellationToken ct) =>
        workflow.CompleteDeviceLoginAsync(request, ct);
}
