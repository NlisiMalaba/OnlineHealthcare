using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Identity.Events;

namespace HealthPlatform.Domain.Identity;

public sealed class Patient : Entity
{
    private Patient()
    {
        FullName = string.Empty;
    }

    public Guid UserId { get; private set; }

    public string FullName { get; private set; }

    public PatientAuthProvider AuthProvider { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? Email { get; private set; }

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
}
