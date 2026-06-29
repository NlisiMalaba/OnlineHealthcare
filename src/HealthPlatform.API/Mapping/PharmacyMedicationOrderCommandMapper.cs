using HealthPlatform.API.Requests.Pharmacy;
using HealthPlatform.Application.PharmacyOrders.ConfirmMedicationOrder;
using HealthPlatform.Application.PharmacyOrders.CompleteMedicationOrderFulfillment;
using HealthPlatform.Application.PharmacyOrders.MarkMedicationOrderDispatched;
using HealthPlatform.Application.PharmacyOrders.RejectMedicationOrder;
using HealthPlatform.Application.PharmacyOrders.RequestMedicationOrderClarification;

namespace HealthPlatform.API.Mapping;

public static class PharmacyMedicationOrderCommandMapper
{
    public static ConfirmMedicationOrderCommand ToConfirmCommand(Guid orderId) =>
        new(orderId);

    public static RejectMedicationOrderCommand ToRejectCommand(Guid orderId, RejectMedicationOrderRequest request) =>
        new(orderId, request.Reason);

    public static RequestMedicationOrderClarificationCommand ToClarificationCommand(
        Guid orderId,
        RequestMedicationOrderClarificationRequest request) =>
        new(orderId, request.Message);

    public static MarkMedicationOrderDispatchedCommand ToDispatchedCommand(Guid orderId) =>
        new(orderId);

    public static CompleteMedicationOrderFulfillmentCommand ToFulfillmentCommand(Guid orderId) =>
        new(orderId);
}
