using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Domain.ValueObjects;

namespace HealthPlatform.Domain.Identity;

public sealed class Doctor : Entity
{
    private readonly List<DoctorAvailabilitySlot> _availabilitySlots = [];

    private Doctor()
    {
        FullName = string.Empty;
        LicenseNumber = string.Empty;
        Specialty = string.Empty;
        ClinicAddress = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
    }

    public Guid UserId { get; private set; }

    public string FullName { get; private set; }

    public string LicenseNumber { get; private set; }

    public string Specialty { get; private set; }

    public int YearsOfExperience { get; private set; }

    public string ClinicAddress { get; private set; }

    public GeoPoint? ClinicLocation { get; private set; }

    public decimal VirtualFee { get; private set; }

    public decimal PhysicalFee { get; private set; }

    public string? Bio { get; private set; }

    public string Email { get; private set; }

    public string PhoneNumber { get; private set; }

    public string? ProfilePhotoStorageKey { get; private set; }

    public string? CredentialsStorageKey { get; private set; }

    public DoctorVerificationStatus VerificationStatus { get; private set; }

    public string? RejectionReason { get; private set; }

    public decimal AverageRating { get; private set; }

    public int TotalReviews { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public IReadOnlyCollection<DoctorAvailabilitySlot> AvailabilitySlots => _availabilitySlots;

    public static Doctor Register(
        Guid doctorId,
        Guid userId,
        string fullName,
        string licenseNumber,
        string specialty,
        int yearsOfExperience,
        string clinicAddress,
        GeoPoint? clinicLocation,
        decimal virtualFee,
        decimal physicalFee,
        string? bio,
        string email,
        string phoneNumber,
        string? profilePhotoStorageKey,
        string? credentialsStorageKey,
        IEnumerable<DoctorAvailabilitySlot> availabilitySlots)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(licenseNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(specialty);
        ArgumentException.ThrowIfNullOrWhiteSpace(clinicAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);

        if (yearsOfExperience < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(yearsOfExperience));
        }

        if (virtualFee < 0 || physicalFee < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(virtualFee));
        }

        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        var slots = availabilitySlots.ToList();
        if (slots.Count == 0)
        {
            throw new ArgumentException("At least one availability slot is required.", nameof(availabilitySlots));
        }

        var doctor = new Doctor
        {
            Id = doctorId,
            UserId = userId,
            FullName = fullName.Trim(),
            LicenseNumber = licenseNumber.Trim().ToUpperInvariant(),
            Specialty = specialty.Trim(),
            YearsOfExperience = yearsOfExperience,
            ClinicAddress = clinicAddress.Trim(),
            ClinicLocation = clinicLocation,
            VirtualFee = virtualFee,
            PhysicalFee = physicalFee,
            Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = phoneNumber.Trim(),
            ProfilePhotoStorageKey = profilePhotoStorageKey,
            CredentialsStorageKey = credentialsStorageKey,
            VerificationStatus = DoctorVerificationStatus.Pending,
            AverageRating = 0m,
            TotalReviews = 0
        };

        doctor._availabilitySlots.AddRange(slots);
        doctor.RaiseDomainEvent(new DoctorRegisteredDomainEvent(
            doctor.Id,
            doctor.LicenseNumber,
            doctor.FullName));

        return doctor;
    }

    public void VerifyLicense()
    {
        EnsurePendingForVerificationTransition();

        VerificationStatus = DoctorVerificationStatus.Verified;
        RejectionReason = null;
        Touch();
        RaiseDomainEvent(new DoctorLicenseVerifiedDomainEvent(Id, UserId, FullName));
    }

    public void RejectLicense(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        EnsurePendingForVerificationTransition();

        RejectionReason = reason.Trim();
        VerificationStatus = DoctorVerificationStatus.Rejected;
        Touch();
        RaiseDomainEvent(new DoctorLicenseRejectedDomainEvent(Id, UserId, FullName, RejectionReason));
    }

    private void EnsurePendingForVerificationTransition()
    {
        if (VerificationStatus != DoctorVerificationStatus.Pending)
        {
            throw new InvalidDoctorVerificationStatusException(VerificationStatus);
        }
    }
}
