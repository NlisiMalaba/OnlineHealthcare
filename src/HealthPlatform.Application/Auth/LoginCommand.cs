using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Auth;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponseDto>;
