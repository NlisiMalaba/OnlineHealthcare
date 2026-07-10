using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Labs.ApprovePatientLabOrder;

public sealed record ApprovePatientLabOrderCommand(Guid LabOrderId) : ICommand<LabOrderDto>;
