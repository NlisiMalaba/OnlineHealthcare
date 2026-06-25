using HealthPlatform.Application.Exceptions;
using HealthPlatform.Domain.Prescriptions;

namespace HealthPlatform.Application.Prescriptions.Dispensing;

public sealed class PrescriptionDispensingGuard(
    IPrescriptionRepository prescriptionRepository,
    TimeProvider timeProvider) : IPrescriptionDispensingGuard
{
    public async Task<PrescriptionDto> DispenseForMedicationOrderAsync(
        Guid prescriptionId,
        Guid patientId,
        CancellationToken ct)
    {
        var prescription = await prescriptionRepository.GetByIdForPatientAsync(prescriptionId, patientId, ct);
        if (prescription is null)
        {
            throw new DomainException(
                PrescriptionErrorCodes.PrescriptionRequired,
                "A valid prescription is required to place a medication order.");
        }

        try
        {
            prescription.MarkDispensed(timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (PrescriptionDispensedException)
        {
            throw new DomainException(
                PrescriptionErrorCodes.PrescriptionDispensed,
                "This prescription has already been used for a medication order.");
        }
        catch (PrescriptionExpiredException)
        {
            throw new DomainException(
                PrescriptionErrorCodes.PrescriptionExpired,
                "The prescription has expired and cannot be used to place an order.");
        }
        catch (PrescriptionNotEligibleException)
        {
            throw new DomainException(
                PrescriptionErrorCodes.PrescriptionRequired,
                "A valid prescription is required to place a medication order.");
        }

        await prescriptionRepository.UpdateAsync(prescription, ct);
        return prescription.ToDto();
    }
}
