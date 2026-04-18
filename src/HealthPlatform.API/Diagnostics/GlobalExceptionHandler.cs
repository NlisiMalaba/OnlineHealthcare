using System.Diagnostics;
using System.Net.Mime;
using FluentValidation;
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
        var (status, code, message, details) = MapException(exception);
        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception mapped to {Code}", code);
        }

        var traceId = Activity.Current?.TraceId.ToHexString()
            ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;

        var envelope = new ErrorResponseDto(new ErrorBodyDto(code, message, details, traceId));
        await httpContext.Response.WriteAsJsonAsync(envelope, ct);
        return true;
    }

    private static (int Status, string Code, string Message, object? Details) MapException(Exception exception) =>
        exception switch
        {
            AppHttpException ex => (ex.StatusCode, ex.ErrorCode, ex.Message, null),
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR",
                "One or more validation errors occurred.",
                ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Code, ex.Message, null),
            ConflictException ex => (StatusCodes.Status409Conflict, ex.Code, ex.Message, null),
            AccessDeniedException ex => (StatusCodes.Status403Forbidden, ex.Code, ex.Message, null),
            DomainException ex => (StatusCodes.Status422UnprocessableEntity, ex.Code, ex.Message, null),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.", null)
        };
}
