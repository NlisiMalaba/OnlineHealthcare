using HealthPlatform.API.Requests.Pharmacy;
using HealthPlatform.Application.PharmacyOrders.CreateMedicationOrder;

namespace HealthPlatform.API.Mapping;

public static class CreateMedicationOrderCommandMapper
{
    public static CreateMedicationOrderCommand ToCommand(CreateMedicationOrderRequest request) =>
        new(
            request.PrescriptionId,
            request.PharmacyId,
            request.MedicationSku,
            request.DeliveryType,
            request.DeliveryAddress);
}
