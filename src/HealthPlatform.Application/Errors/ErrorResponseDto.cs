using System.Text.Json.Serialization;

namespace HealthPlatform.Application.Errors;

/// <summary>
/// Standard API error envelope (Requirements 17.x — machine code + trace correlation).
/// </summary>
public sealed record ErrorResponseDto(
    [property: JsonPropertyName("error")] ErrorBodyDto Error);

public sealed record ErrorBodyDto(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("details")] object? Details,
    [property: JsonPropertyName("trace_id")] string TraceId);
