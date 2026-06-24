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

    public decimal LateCancellationRetentionPercent { get; private set; }

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
            LateCancellationRetentionPercent = 100m,
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

    public void UpdateProfile(
        decimal? virtualFee,
        decimal? physicalFee,
        string? bio,
        string? profilePhotoStorageKey,
        string? credentialsStorageKey)
    {
        var profileChanged = false;

        if (virtualFee.HasValue)
        {
            if (virtualFee.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(virtualFee));
            }

            if (VirtualFee != virtualFee.Value)
            {
                VirtualFee = virtualFee.Value;
                profileChanged = true;
            }
        }

        if (physicalFee.HasValue)
        {
            if (physicalFee.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(physicalFee));
            }

            if (PhysicalFee != physicalFee.Value)
            {
                PhysicalFee = physicalFee.Value;
                profileChanged = true;
            }
        }

        if (bio is not null)
        {
            var normalizedBio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
            if (Bio != normalizedBio)
            {
                Bio = normalizedBio;
                profileChanged = true;
            }
        }

        if (profilePhotoStorageKey is not null && ProfilePhotoStorageKey != profilePhotoStorageKey)
        {
            ProfilePhotoStorageKey = profilePhotoStorageKey;
            profileChanged = true;
        }

        if (credentialsStorageKey is not null && CredentialsStorageKey != credentialsStorageKey)
        {
            CredentialsStorageKey = credentialsStorageKey;
            profileChanged = true;
        }

        if (!profileChanged)
        {
            return;
        }

        Touch();
        RaiseDomainEvent(new DoctorProfileUpdatedDomainEvent(Id));
    }

    public bool ApplyAvailabilityReplacement(IReadOnlyList<DoctorAvailabilitySlot> replacementSlots)
    {
        ArgumentNullException.ThrowIfNull(replacementSlots);

        if (replacementSlots.Count == 0)
        {
            throw new ArgumentException("At least one availability slot is required.", nameof(replacementSlots));
        }

        if (AvailabilitySlotsMatch(_availabilitySlots, replacementSlots))
        {
            return false;
        }

        Touch();
        RaiseDomainEvent(new DoctorAvailabilityChangedDomainEvent(Id));
        return true;
    }

    public void SetAvailabilitySlots(IReadOnlyList<DoctorAvailabilitySlot> replacementSlots)
    {
        _availabilitySlots.Clear();
        _availabilitySlots.AddRange(replacementSlots);
    }

    public DoctorAvailabilitySlot AddAvailabilitySlot(
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int slotDurationMinutes,
        DoctorAppointmentType appointmentType)
    {
        var slot = DoctorAvailabilitySlot.Create(
            Id,
            dayOfWeek,
            startTime,
            endTime,
            slotDurationMinutes,
            appointmentType);

        _availabilitySlots.Add(slot);
        Touch();
        RaiseDomainEvent(new DoctorAvailabilityChangedDomainEvent(Id));
        return slot;
    }

    public DoctorAvailabilitySlot GetAvailabilitySlot(Guid slotId) =>
        _availabilitySlots.SingleOrDefault(slot => slot.Id == slotId)
        ?? throw new KeyNotFoundException("Availability slot was not found.");

    public bool UpdateAvailabilitySlot(
        Guid slotId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int slotDurationMinutes,
        DoctorAppointmentType appointmentType)
    {
        var slot = GetAvailabilitySlot(slotId);
        var changed = slot.Update(dayOfWeek, startTime, endTime, slotDurationMinutes, appointmentType);
        if (!changed)
        {
            return false;
        }

        Touch();
        RaiseDomainEvent(new DoctorAvailabilityChangedDomainEvent(Id));
        return true;
    }

    public bool RemoveAvailabilitySlot(Guid slotId)
    {
        var slot = _availabilitySlots.SingleOrDefault(s => s.Id == slotId);
        if (slot is null)
        {
            return false;
        }

        _availabilitySlots.Remove(slot);
        Touch();
        RaiseDomainEvent(new DoctorAvailabilityChangedDomainEvent(Id));
        return true;
    }

    private void EnsurePendingForVerificationTransition()
    {
        if (VerificationStatus != DoctorVerificationStatus.Pending)
        {
            throw new InvalidDoctorVerificationStatusException(VerificationStatus);
        }
    }

    private static bool AvailabilitySlotsMatch(
        IReadOnlyCollection<DoctorAvailabilitySlot> current,
        IReadOnlyList<DoctorAvailabilitySlot> replacement)
    {
        if (current.Count != replacement.Count)
        {
            return false;
        }

        var currentSnapshot = current
            .Select(SlotSnapshot.From)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToList();

        var replacementSnapshot = replacement
            .Select(SlotSnapshot.From)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToList();

        return currentSnapshot.SequenceEqual(replacementSnapshot);
    }

    private readonly record struct SlotSnapshot(
        DayOfWeek DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime,
        int SlotDurationMinutes,
        DoctorAppointmentType AppointmentType,
        bool IsActive)
    {
        public static SlotSnapshot From(DoctorAvailabilitySlot slot) =>
            new(
                slot.DayOfWeek,
                slot.StartTime,
                slot.EndTime,
                slot.SlotDurationMinutes,
                slot.AppointmentType,
                slot.IsActive);
    }
}
