using System.Globalization;
using System.Text;
using HealthPlatform.Application.Payments;
using HealthPlatform.Domain.Payments;

namespace HealthPlatform.Infrastructure.Payments;

public sealed class TextPaymentReceiptGenerator : IPaymentReceiptGenerator
{
    public byte[] Generate(Payment payment)
    {
        var builder = new StringBuilder();
        builder.AppendLine("HEALTH PLATFORM PAYMENT RECEIPT");
        builder.AppendLine(new string('-', 40));
        builder.AppendLine(CultureInfo.InvariantCulture, $"Receipt ID: {payment.Id}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Patient ID: {payment.PatientId}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Completed: {(payment.CompletedAtUtc ?? payment.FailedAtUtc ?? payment.CreatedAtUtc):O}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Amount: {payment.AmountMinorUnits} {payment.Currency}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Method: {payment.PaymentMethod}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Gateway: {payment.Gateway}");

        if (!string.IsNullOrWhiteSpace(payment.GatewayReference))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"Reference: {payment.GatewayReference}");
        }

        if (payment.AppointmentId is { } appointmentId)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"Appointment: {appointmentId}");
        }

        if (payment.MedicationOrderId is { } medicationOrderId)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"Medication order: {medicationOrderId}");
        }

        if (payment.LabOrderId is { } labOrderId)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"Lab order: {labOrderId}");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}
