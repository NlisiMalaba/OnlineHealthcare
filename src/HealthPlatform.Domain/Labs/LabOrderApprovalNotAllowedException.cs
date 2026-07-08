namespace HealthPlatform.Domain.Labs;

public sealed class LabOrderApprovalNotAllowedException(LabOrderRequestSource source, LabOrderStatus status)
    : Exception($"Cannot approve lab order from source '{source}' while status is '{status}'.");
