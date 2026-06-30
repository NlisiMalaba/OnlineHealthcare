using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Insurance;
using HealthPlatform.Application.Payments.CreditLine;
using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Application.Storage;
using MediatR;

namespace HealthPlatform.Application.Payments.ListPatientTransactionHistory;

public sealed class ListPatientTransactionHistoryQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IPaymentRepository paymentRepository,
    ICreditLineTransactionRepository creditLineTransactionRepository,
    IInstalmentPlanRepository instalmentPlanRepository,
    IInsuranceClaimRepository insuranceClaimRepository,
    IStorageService storageService)
    : IRequestHandler<ListPatientTransactionHistoryQuery, IReadOnlyList<PatientTransactionHistoryItemDto>>
{
    public async Task<IReadOnlyList<PatientTransactionHistoryItemDto>> Handle(
        ListPatientTransactionHistoryQuery request,
        CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var items = new List<PatientTransactionHistoryItemDto>();

        var payments = await paymentRepository.ListForPatientAsync(patient.Id, ct);
        foreach (var payment in payments)
        {
            string? receiptUrl = null;
            if (!string.IsNullOrWhiteSpace(payment.ReceiptStorageKey))
            {
                receiptUrl = await storageService.GetSignedReadUrlAsync(payment.ReceiptStorageKey, ct);
            }

            items.Add(PaymentMappings.ToHistoryItem(payment, receiptUrl));
        }

        var creditTransactions = await creditLineTransactionRepository.ListForPatientAsync(patient.Id, ct);
        items.AddRange(creditTransactions.Select(PaymentMappings.ToHistoryItem));

        var instalmentPlans = await instalmentPlanRepository.ListForPatientAsync(patient.Id, ct);
        items.AddRange(instalmentPlans.Select(PaymentMappings.ToHistoryItem));

        var insuranceClaims = await insuranceClaimRepository.ListForPatientAsync(patient.Id, ct);
        items.AddRange(insuranceClaims.Select(PaymentMappings.ToHistoryItem));

        return items
            .OrderByDescending(item => item.OccurredAtUtc)
            .ToList();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");
    }
}
