using FluentValidation.TestHelper;
using HealthPlatform.Application.PharmacyOrders.RejectMedicationOrder;
using HealthPlatform.Application.PharmacyOrders.RequestMedicationOrderClarification;
using Xunit;

namespace HealthPlatform.Tests.Unit.PharmacyOrders;

public sealed class MedicationOrderWorkflowValidatorTests
{
    [Fact]
    public void Reject_validator_requires_reason()
    {
        var validator = new RejectMedicationOrderCommandValidator();
        var result = validator.TestValidate(new RejectMedicationOrderCommand(Guid.CreateVersion7(), ""));
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Clarification_validator_requires_message()
    {
        var validator = new RequestMedicationOrderClarificationCommandValidator();
        var result = validator.TestValidate(
            new RequestMedicationOrderClarificationCommand(Guid.CreateVersion7(), ""));
        result.ShouldHaveValidationErrorFor(x => x.Message);
    }
}
