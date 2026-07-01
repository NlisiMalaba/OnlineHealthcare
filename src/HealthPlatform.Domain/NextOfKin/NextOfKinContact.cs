using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.NextOfKin;

public sealed class NextOfKinContact : Entity
{
    private NextOfKinContact()
    {
        FullName = string.Empty;
        Relationship = string.Empty;
        PhoneNumber = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public string FullName { get; private set; }

    public string Relationship { get; private set; }

    public string PhoneNumber { get; private set; }

    public string? Email { get; private set; }

    public bool IsMentalHealthContact { get; private set; }

    public static NextOfKinContact Create(
        Guid patientId,
        string fullName,
        string relationship,
        string phoneNumber,
        string? email,
        bool isMentalHealthContact)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (string.IsNullOrWhiteSpace(relationship))
        {
            throw new ArgumentException("Relationship is required.", nameof(relationship));
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));
        }

        return new NextOfKinContact
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            FullName = fullName.Trim(),
            Relationship = relationship.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            IsMentalHealthContact = isMentalHealthContact
        };
    }

    public void Update(
        string fullName,
        string relationship,
        string phoneNumber,
        string? email,
        bool isMentalHealthContact)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (string.IsNullOrWhiteSpace(relationship))
        {
            throw new ArgumentException("Relationship is required.", nameof(relationship));
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));
        }

        FullName = fullName.Trim();
        Relationship = relationship.Trim();
        PhoneNumber = phoneNumber.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        IsMentalHealthContact = isMentalHealthContact;
        Touch();
    }
}
