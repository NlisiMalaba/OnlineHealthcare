using System.Diagnostics;
using System.Net.Mime;
using HealthPlatform.Application.Errors;
using HealthPlatform.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace HealthPlatform.API.Diagnostics;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var (status, code, message) = MapException(exception);
        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception mapped to {Code}", code);
        }

        var traceId = Activity.Current?.TraceId.ToHexString()
            ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;

        var envelope = new ErrorResponseDto(new ErrorBodyDto(code, message, null, traceId));
        await httpContext.Response.WriteAsJsonAsync(envelope, ct);
        return true;
    }

    private static (int Status, string Code, string Message) MapException(Exception exception) =>
        exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Code, ex.Message),
            ConflictException ex => (StatusCodes.Status409Conflict, ex.Code, ex.Message),
            AccessDeniedException ex => (StatusCodes.Status403Forbidden, ex.Code, ex.Message),
            DomainException ex => (StatusCodes.Status422UnprocessableEntity, ex.Code, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
        };
}
