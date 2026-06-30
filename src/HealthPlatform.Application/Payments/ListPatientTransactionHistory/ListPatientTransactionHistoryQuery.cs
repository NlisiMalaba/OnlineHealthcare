using MediatR;

namespace HealthPlatform.Application.Payments.ListPatientTransactionHistory;

public sealed record ListPatientTransactionHistoryQuery : IRequest<IReadOnlyList<PatientTransactionHistoryItemDto>>;
