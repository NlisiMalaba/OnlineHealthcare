using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.PharmacyOrders.CompleteMedicationOrderFulfillment;
using HealthPlatform.Application.PharmacyOrders.ConfirmMedicationOrder;
using HealthPlatform.Application.PharmacyOrders.MarkMedicationOrderDispatched;
using HealthPlatform.Application.PharmacyOrders.RejectMedicationOrder;
using HealthPlatform.Application.PharmacyOrders.RequestMedicationOrderClarification;
using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Tests.Support;
using Xunit;

namespace HealthPlatform.Tests.Unit.PharmacyOrders;

public sealed class MedicationOrderWorkflowCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Confirm_delivery_order_assigns_agent_tracking_and_notifies_patient()
    {
        var context = await MedicationOrderTestData.SeedPendingDeliveryOrderAsync(_host);
        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        var order = await _host.Sender.Send(
            new ConfirmMedicationOrderCommand(context.Order.Id),
            CancellationToken.None);

        Assert.Equal("confirmed", order.Status);
        Assert.NotNull(order.DeliveryAgentName);
        Assert.NotNull(order.TrackingUrl);
        Assert.Contains(context.Order.Id.ToString(), order.TrackingUrl, StringComparison.OrdinalIgnoreCase);

        Assert.Single(_host.MedicationOrderPatientNotifier.Notifications);
        Assert.Equal(context.Patient.UserId, _host.MedicationOrderPatientNotifier.Notifications[0].PatientUserId);
        Assert.Equal("confirmed", _host.MedicationOrderPatientNotifier.Notifications[0].NewStatus);
        Assert.Equal(order.TrackingUrl, _host.MedicationOrderPatientNotifier.Notifications[0].TrackingUrl);
    }

    [Fact]
    public async Task Confirm_pickup_order_does_not_assign_delivery_agent()
    {
        var context = await MedicationOrderTestData.SeedPendingPickupOrderAsync(_host);
        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        var order = await _host.Sender.Send(
            new ConfirmMedicationOrderCommand(context.Order.Id),
            CancellationToken.None);

        Assert.Equal("confirmed", order.Status);
        Assert.Null(order.DeliveryAgentName);
        Assert.Null(order.TrackingUrl);
    }

    [Fact]
    public async Task Reject_order_notifies_patient_with_alternative_pharmacies()
    {
        var context = await MedicationOrderTestData.SeedPendingDeliveryOrderAsync(_host);
        var alternativePharmacyId = Guid.CreateVersion7();
        _host.SearchService.SeedPharmacySearchResults(
        [
            new(context.Pharmacy.Id, "Rejecting Pharmacy", "1 Main St", true, null),
            new(alternativePharmacyId, "Alt Pharmacy", "2 Second St", true, null)
        ]);

        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        var order = await _host.Sender.Send(
            new RejectMedicationOrderCommand(context.Order.Id, "Out of stock after verification"),
            CancellationToken.None);

        Assert.Equal("rejected", order.Status);
        Assert.Equal("Out of stock after verification", order.RejectionReason);

        var notification = Assert.Single(_host.MedicationOrderPatientNotifier.Notifications);
        Assert.Equal("rejected", notification.NewStatus);
        Assert.NotNull(notification.Alternatives);
        Assert.Contains(notification.Alternatives, alt => alt.PharmacyId == alternativePharmacyId);
        Assert.DoesNotContain(notification.Alternatives, alt => alt.PharmacyId == context.Pharmacy.Id);
    }

    [Fact]
    public async Task Request_clarification_moves_order_to_clarification_requested()
    {
        var context = await MedicationOrderTestData.SeedPendingDeliveryOrderAsync(_host);
        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        var order = await _host.Sender.Send(
            new RequestMedicationOrderClarificationCommand(context.Order.Id, "Please confirm delivery entrance."),
            CancellationToken.None);

        Assert.Equal("clarificationrequested", order.Status);
        Assert.Equal("Please confirm delivery entrance.", order.ClarificationMessage);
    }

    [Fact]
    public async Task Delivery_fulfillment_flow_reaches_delivered_status()
    {
        var context = await MedicationOrderTestData.SeedPendingDeliveryOrderAsync(_host);
        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        await _host.Sender.Send(new ConfirmMedicationOrderCommand(context.Order.Id), CancellationToken.None);
        await _host.Sender.Send(new MarkMedicationOrderDispatchedCommand(context.Order.Id), CancellationToken.None);

        var completed = await _host.Sender.Send(
            new CompleteMedicationOrderFulfillmentCommand(context.Order.Id),
            CancellationToken.None);

        Assert.Equal("delivered", completed.Status);
        Assert.Equal(3, _host.MedicationOrderPatientNotifier.Notifications.Count);
    }

    [Fact]
    public async Task Pickup_fulfillment_marks_order_delivered_after_confirmation()
    {
        var context = await MedicationOrderTestData.SeedPendingPickupOrderAsync(_host);
        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        await _host.Sender.Send(new ConfirmMedicationOrderCommand(context.Order.Id), CancellationToken.None);

        var completed = await _host.Sender.Send(
            new CompleteMedicationOrderFulfillmentCommand(context.Order.Id),
            CancellationToken.None);

        Assert.Equal("delivered", completed.Status);
    }
}
