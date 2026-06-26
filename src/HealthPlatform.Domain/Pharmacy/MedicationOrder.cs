using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Pharmacy.Events;

namespace HealthPlatform.Domain.Pharmacy;

public sealed class MedicationOrder : Entity
{
    private MedicationOrder()
    {
        MedicationSku = string.Empty;
        MedicationName = string.Empty;
        Dosage = string.Empty;
        Frequency = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public Guid PharmacyId { get; private set; }

    public Guid PrescriptionId { get; private set; }

    public string MedicationSku { get; private set; }

    public string MedicationName { get; private set; }

    public string Dosage { get; private set; }

    public string Frequency { get; private set; }

    public int DurationDays { get; private set; }

    public string? SpecialInstructions { get; private set; }

    public MedicationDeliveryType DeliveryType { get; private set; }

    public string? DeliveryAddress { get; private set; }

    public MedicationOrderStatus Status { get; private set; }

    public static MedicationOrder Place(
        Guid patientId,
        Guid pharmacyId,
        Guid prescriptionId,
        string medicationSku,
        string medicationName,
        string dosage,
        string frequency,
        int durationDays,
        string? specialInstructions,
        MedicationDeliveryType deliveryType,
        string? deliveryAddress)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (pharmacyId == Guid.Empty)
        {
            throw new ArgumentException("Pharmacy id is required.", nameof(pharmacyId));
        }

        if (prescriptionId == Guid.Empty)
        {
            throw new ArgumentException("Prescription id is required.", nameof(prescriptionId));
        }

        if (string.IsNullOrWhiteSpace(medicationSku))
        {
            throw new ArgumentException("Medication sku is required.", nameof(medicationSku));
        }

        if (string.IsNullOrWhiteSpace(medicationName))
        {
            throw new ArgumentException("Medication name is required.", nameof(medicationName));
        }

        if (string.IsNullOrWhiteSpace(dosage))
        {
            throw new ArgumentException("Dosage is required.", nameof(dosage));
        }

        if (string.IsNullOrWhiteSpace(frequency))
        {
            throw new ArgumentException("Frequency is required.", nameof(frequency));
        }

        if (durationDays <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero.", nameof(durationDays));
        }

        if (deliveryType == MedicationDeliveryType.Delivery && string.IsNullOrWhiteSpace(deliveryAddress))
        {
            throw new ArgumentException(
                "Delivery address is required for delivery orders.",
                nameof(deliveryAddress));
        }

        if (deliveryType == MedicationDeliveryType.Pickup && !string.IsNullOrWhiteSpace(deliveryAddress))
        {
            throw new ArgumentException(
                "Delivery address must not be set for pickup orders.",
                nameof(deliveryAddress));
        }

        var normalizedSku = medicationSku.Trim();
        var normalizedName = medicationName.Trim();
        var normalizedDosage = dosage.Trim();
        var normalizedFrequency = frequency.Trim();
        var normalizedAddress = string.IsNullOrWhiteSpace(deliveryAddress) ? null : deliveryAddress.Trim();
        var normalizedInstructions = string.IsNullOrWhiteSpace(specialInstructions)
            ? null
            : specialInstructions.Trim();

        var order = new MedicationOrder
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            PharmacyId = pharmacyId,
            PrescriptionId = prescriptionId,
            MedicationSku = normalizedSku,
            MedicationName = normalizedName,
            Dosage = normalizedDosage,
            Frequency = normalizedFrequency,
            DurationDays = durationDays,
            SpecialInstructions = normalizedInstructions,
            DeliveryType = deliveryType,
            DeliveryAddress = normalizedAddress,
            Status = MedicationOrderStatus.Pending
        };

        order.RaiseDomainEvent(new MedicationOrderPlacedDomainEvent(
            order.Id,
            order.PatientId,
            order.PharmacyId,
            order.PrescriptionId,
            order.MedicationSku,
            order.MedicationName,
            order.Dosage,
            order.Frequency,
            order.DurationDays,
            order.SpecialInstructions,
            order.DeliveryType,
            order.DeliveryAddress));

        return order;
    }
}
