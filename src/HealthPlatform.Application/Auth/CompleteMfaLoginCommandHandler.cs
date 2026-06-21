using MediatR;

namespace HealthPlatform.Application.Auth;

public sealed class CompleteMfaLoginCommandHandler(IAuthLoginWorkflow workflow)
    : IRequestHandler<CompleteMfaLoginCommand, LoginResponseDto>
{
    public Task<LoginResponseDto> Handle(CompleteMfaLoginCommand request, CancellationToken ct) =>
        workflow.CompleteMfaAsync(request, ct);
}
