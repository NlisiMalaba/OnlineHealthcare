using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Domain.ValueObjects;

namespace HealthPlatform.Domain.Identity;

public sealed class Pharmacy : Entity
{
    private Pharmacy()
    {
        Name = string.Empty;
        Address = string.Empty;
        ContactEmail = string.Empty;
        ContactPhone = string.Empty;
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public string Address { get; private set; }

    public GeoPoint? Location { get; private set; }

    public string ContactEmail { get; private set; }

    public string ContactPhone { get; private set; }

    public string? LogoStorageKey { get; private set; }

    public PharmacyVerificationStatus VerificationStatus { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public static Pharmacy Register(
        Guid pharmacyId,
        Guid userId,
        string name,
        string address,
        GeoPoint? location,
        string contactEmail,
        string contactPhone,
        string? logoStorageKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactPhone);

        if (pharmacyId == Guid.Empty)
        {
            throw new ArgumentException("Pharmacy id is required.", nameof(pharmacyId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        var pharmacy = new Pharmacy
        {
            Id = pharmacyId,
            UserId = userId,
            Name = name.Trim(),
            Address = address.Trim(),
            Location = location,
            ContactEmail = contactEmail.Trim().ToLowerInvariant(),
            ContactPhone = contactPhone.Trim(),
            LogoStorageKey = logoStorageKey,
            VerificationStatus = PharmacyVerificationStatus.Pending
        };

        pharmacy.RaiseDomainEvent(new PharmacyRegisteredDomainEvent(
            pharmacy.Id,
            pharmacy.Name,
            pharmacy.ContactEmail));

        return pharmacy;
    }

    public void UpdateProfile(
        string? name,
        string? address,
        GeoPoint? location,
        string? contactPhone,
        string? logoStorageKey)
    {
        var profileChanged = false;

        if (name is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            var normalizedName = name.Trim();
            if (Name != normalizedName)
            {
                Name = normalizedName;
                profileChanged = true;
            }
        }

        if (address is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(address);
            var normalizedAddress = address.Trim();
            if (Address != normalizedAddress)
            {
                Address = normalizedAddress;
                profileChanged = true;
            }
        }

        if (location is not null && Location != location)
        {
            Location = location;
            profileChanged = true;
        }

        if (contactPhone is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contactPhone);
            var normalizedPhone = contactPhone.Trim();
            if (ContactPhone != normalizedPhone)
            {
                ContactPhone = normalizedPhone;
                profileChanged = true;
            }
        }

        if (logoStorageKey is not null && LogoStorageKey != logoStorageKey)
        {
            LogoStorageKey = logoStorageKey;
            profileChanged = true;
        }

        if (!profileChanged)
        {
            return;
        }

        Touch();
        RaiseDomainEvent(new PharmacyProfileUpdatedDomainEvent(Id));
    }
}
