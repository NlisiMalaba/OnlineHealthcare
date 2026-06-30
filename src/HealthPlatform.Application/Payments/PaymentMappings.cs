using HealthPlatform.Domain.Insurance;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Domain.Payments.CreditLine;
using HealthPlatform.Domain.Payments.Instalments;

namespace HealthPlatform.Application.Payments;

public static class PaymentMappings
{
    public static PatientTransactionHistoryItemDto ToHistoryItem(
        Payment payment,
        string? receiptUrl) =>
        new(
            payment.Id,
            ResolvePaymentCategory(payment),
            ResolvePaymentDescription(payment),
            payment.AmountMinorUnits,
            payment.Currency,
            payment.Status.ToString(),
            payment.CompletedAtUtc,
            payment.Id,
            receiptUrl);

    public static PatientTransactionHistoryItemDto ToHistoryItem(CreditLineTransaction transaction) =>
        new(
            transaction.Id,
            PatientTransactionCategory.CreditLine,
            "Credit line charge",
            transaction.AmountMinorUnits,
            transaction.Currency,
            transaction.TransactionType.ToString(),
            transaction.CreatedAtUtc,
            null,
            null);

    public static PatientTransactionHistoryItemDto ToHistoryItem(InstalmentPlan plan) =>
        new(
            plan.Id,
            PatientTransactionCategory.InstalmentPlan,
            $"Instalment plan ({plan.TotalInstalments} payments)",
            plan.TotalRepayableMinorUnits,
            plan.Currency,
            plan.Status.ToString(),
            plan.CreatedAtUtc,
            null,
            null);

    public static PatientTransactionHistoryItemDto ToHistoryItem(InsuranceClaim claim) =>
        new(
            claim.Id,
            PatientTransactionCategory.InsuranceClaim,
            $"Insurance claim ({claim.ClaimType})",
            claim.AmountMinorUnits,
            claim.Currency,
            claim.Status.ToString(),
            claim.CreatedAtUtc,
            null,
            null);

    private static PatientTransactionCategory ResolvePaymentCategory(Payment payment)
    {
        if (payment.AppointmentId is not null)
        {
            return PatientTransactionCategory.ConsultationFee;
        }

        if (payment.MedicationOrderId is not null)
        {
            return PatientTransactionCategory.MedicationCost;
        }

        if (payment.LabOrderId is not null)
        {
            return PatientTransactionCategory.LabTestCharge;
        }

        return PatientTransactionCategory.ConsultationFee;
    }

    private static string ResolvePaymentDescription(Payment payment) =>
        payment.PaymentMethod switch
        {
            PaymentMethod.CreditLine => "Consultation fee (credit line)",
            PaymentMethod.Instalment => "Healthcare expense (instalment plan)",
            PaymentMethod.MobileMoney => "Consultation fee (mobile money)",
            _ => "Consultation fee"
        };
}
