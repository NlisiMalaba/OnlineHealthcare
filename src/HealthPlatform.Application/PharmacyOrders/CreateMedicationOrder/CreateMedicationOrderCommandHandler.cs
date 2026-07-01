using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.Dispensing;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Pharmacy;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.CreateMedicationOrder;

public sealed class CreateMedicationOrderCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IPharmacyRepository pharmacyRepository,
    IPrescriptionRepository prescriptionRepository,
    IPrescriptionDispensingGuard dispensingGuard,
    IPharmacyStockAvailabilityService stockAvailabilityService,
    IMedicationOrderRepository medicationOrderRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    IPrescriptionDomainEventPublisher prescriptionDomainEventPublisher)
    : IRequestHandler<CreateMedicationOrderCommand, MedicationOrderDto>
{
    public async Task<MedicationOrderDto> Handle(CreateMedicationOrderCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);

        var prescription = await prescriptionRepository.GetByIdForPatientAsync(request.PrescriptionId, patient.Id, ct)
            ?? throw new DomainException(
                PrescriptionErrorCodes.PrescriptionRequired,
                "A valid prescription is required to place a medication order.");

        var pharmacy = await pharmacyRepository.GetByIdAsync(request.PharmacyId, ct)
            ?? throw new NotFoundException(
                PharmacyErrorCodes.PharmacyNotFound,
                "Pharmacy was not found.");

        if (pharmacy.VerificationStatus != PharmacyVerificationStatus.Verified)
        {
            throw new NotFoundException(
                PharmacyErrorCodes.PharmacyNotFound,
                "Pharmacy was not found.");
        }

        var medicationSku = request.MedicationSku.Trim();
        var hasStock = await stockAvailabilityService.HasMedicationInStockAsync(
            request.PharmacyId,
            medicationSku,
            ct);

        if (!hasStock)
        {
            throw new DomainException(
                PharmacyErrorCodes.MedicationOutOfStock,
                "The selected pharmacy does not have the prescribed medication in stock.");
        }

        var dispensedPrescription = await dispensingGuard.PrepareDispenseForMedicationOrderAsync(
            request.PrescriptionId,
            patient.Id,
            ct);

        var order = MedicationOrder.Place(
            patient.Id,
            pharmacy.Id,
            prescription.Id,
            medicationSku,
            prescription.MedicationName,
            prescription.Dosage,
            prescription.Frequency,
            prescription.DurationDays,
            prescription.SpecialInstructions,
            request.DeliveryType,
            request.DeliveryAddress);

        await medicationOrderRepository.AddWithDispensedPrescriptionAsync(order, dispensedPrescription, ct);
        await PublishPendingEventsAsync(order, ct);
        await prescriptionDomainEventPublisher.PublishPendingAsync(dispensedPrescription, ct);

        return order.ToDto();
    }

    private async Task<Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                PharmacyErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }

    private async Task PublishPendingEventsAsync(MedicationOrder order, CancellationToken ct)
    {
        var pendingEvents = order.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        order.ClearDomainEvents();
    }
}
