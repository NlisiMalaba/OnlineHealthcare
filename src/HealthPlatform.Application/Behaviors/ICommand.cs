using MediatR;

namespace HealthPlatform.Application.Behaviors;

/// <summary>
/// Marker for state-changing MediatR requests (transaction boundary in handler).
/// </summary>
public interface ICommand : IRequest;

/// <summary>
/// Command returning a typed response.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>;

/// <summary>
/// Read-only MediatR request (no aggregate mutation in handler).
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;
