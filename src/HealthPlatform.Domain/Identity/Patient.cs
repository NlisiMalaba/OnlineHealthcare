using System.Text.Json;
using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Identity.Events;

namespace HealthPlatform.Domain.Identity;

public sealed class Patient : Entity
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private Patient()
    {
        FullName = string.Empty;
    }

    public Guid UserId { get; private set; }

    public string FullName { get; private set; }

    public PatientAuthProvider AuthProvider { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? Email { get; private set; }

    public DateOnly? DateOfBirth { get; private set; }

    public BloodType? BloodType { get; private set; }

    public List<string> KnownAllergies { get; private set; } = [];

    public List<string> ChronicConditions { get; private set; } = [];

    public string? ProfilePhotoStorageKey { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public static Patient RegisterWithPhone(Guid userId, string fullName, string phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);

        var patient = new Patient
        {
            UserId = userId,
            FullName = fullName.Trim(),
            AuthProvider = PatientAuthProvider.Phone,
            PhoneNumber = phoneNumber.Trim()
        };

        patient.RaiseDomainEvent(new PatientRegisteredDomainEvent(patient.Id));
        return patient;
    }

    public static Patient RegisterWithEmail(Guid userId, string fullName, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var patient = new Patient
        {
            UserId = userId,
            FullName = fullName.Trim(),
            AuthProvider = PatientAuthProvider.Email,
            Email = email.Trim().ToLowerInvariant()
        };

        patient.RaiseDomainEvent(new PatientRegisteredDomainEvent(patient.Id));
        return patient;
    }

    public static Patient RegisterWithSocial(
        Guid userId,
        string fullName,
        PatientAuthProvider provider,
        string? email,
        string? phoneNumber)
    {
        if (provider is not (PatientAuthProvider.Google or PatientAuthProvider.Apple))
        {
            throw new ArgumentException("Social registration requires Google or Apple provider.", nameof(provider));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        var patient = new Patient
        {
            UserId = userId,
            FullName = fullName.Trim(),
            AuthProvider = provider,
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant(),
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim()
        };

        patient.RaiseDomainEvent(new PatientRegisteredDomainEvent(patient.Id));
        return patient;
    }

    public IReadOnlyList<ProfileFieldChange> UpdateProfile(
        string? fullName,
        DateOnly? dateOfBirth,
        BloodType? bloodType,
        IReadOnlyList<string>? knownAllergies,
        IReadOnlyList<string>? chronicConditions,
        string? profilePhotoStorageKey)
    {
        var changes = new List<ProfileFieldChange>();

        if (fullName is not null)
        {
            var trimmed = fullName.Trim();
            if (trimmed.Length == 0)
            {
                throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
            }

            RecordChange(changes, nameof(FullName), FullName, trimmed);
            FullName = trimmed;
        }

        if (dateOfBirth.HasValue)
        {
            RecordChange(changes, nameof(DateOfBirth), FormatDate(DateOfBirth), FormatDate(dateOfBirth));
            DateOfBirth = dateOfBirth;
        }

        if (bloodType.HasValue)
        {
            RecordChange(changes, nameof(BloodType), FormatBloodType(BloodType), FormatBloodType(bloodType));
            BloodType = bloodType;
        }

        if (knownAllergies is not null)
        {
            var normalized = NormalizeStringList(knownAllergies);
            RecordChange(changes, nameof(KnownAllergies), SerializeList(KnownAllergies), SerializeList(normalized));
            KnownAllergies = normalized;
        }

        if (chronicConditions is not null)
        {
            var normalized = NormalizeStringList(chronicConditions);
            RecordChange(
                changes,
                nameof(ChronicConditions),
                SerializeList(ChronicConditions),
                SerializeList(normalized));
            ChronicConditions = normalized;
        }

        if (profilePhotoStorageKey is not null)
        {
            RecordChange(
                changes,
                nameof(ProfilePhotoStorageKey),
                ProfilePhotoStorageKey,
                profilePhotoStorageKey);
            ProfilePhotoStorageKey = profilePhotoStorageKey;
        }

        if (changes.Count > 0)
        {
            Touch();
        }

        return changes;
    }

    private static void RecordChange(
        List<ProfileFieldChange> changes,
        string fieldName,
        string? previousValue,
        string? newValue)
    {
        if (string.Equals(previousValue, newValue, StringComparison.Ordinal))
        {
            return;
        }

        changes.Add(new ProfileFieldChange(fieldName, previousValue, newValue));
    }

    private static List<string> NormalizeStringList(IReadOnlyList<string> values) =>
        values
            .Select(v => v.Trim())
            .Where(v => v.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string? FormatDate(DateOnly? value) =>
        value?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private static string? FormatBloodType(BloodType? value) => value?.ToString();

    private static string SerializeList(IReadOnlyList<string> values) =>
        JsonSerializer.Serialize(values, SerializerOptions);
}
