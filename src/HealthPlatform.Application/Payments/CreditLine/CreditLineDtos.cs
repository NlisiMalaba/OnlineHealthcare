namespace HealthPlatform.Application.Payments.CreditLine;

public sealed record CreditLineDto(
    Guid Id,
    long CreditLimitMinorUnits,
    long OutstandingBalanceMinorUnits,
    long AvailableCreditMinorUnits,
    decimal CreditScore,
    string Currency);

public sealed record CreditLinePaymentDto(
    Guid TransactionId,
    Guid CreditLineId,
    long AmountMinorUnits,
    string Currency,
    long OutstandingBalanceMinorUnits,
    long CreditLimitMinorUnits,
    DateTime RepaymentDueAtUtc,
    bool BalanceWarningEmitted);
