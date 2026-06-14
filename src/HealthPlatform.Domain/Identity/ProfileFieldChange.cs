namespace HealthPlatform.Domain.Identity;

public sealed record ProfileFieldChange(string FieldName, string? PreviousValue, string? NewValue);
