using HealthPlatform.Application.Exceptions;
using HealthPlatform.Domain.Payments.Instalments;
using MediatR;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Application.Payments.Instalments.PreviewInstalmentPlan;

public sealed class PreviewInstalmentPlanQueryHandler(
    IOptions<InstalmentPlanOptions> options)
    : IRequestHandler<PreviewInstalmentPlanQuery, InstalmentPlanPreviewDto>
{
    public Task<InstalmentPlanPreviewDto> Handle(PreviewInstalmentPlanQuery request, CancellationToken ct)
    {
        var settings = options.Value;

        if (request.InstalmentCount > settings.MaxInstalments)
        {
            throw new DomainException(
                InstalmentErrorCodes.InvalidInstalmentPlan,
                $"Instalment count cannot exceed {settings.MaxInstalments}.");
        }

        if (!InstalmentPolicies.MeetsMinimumExpense(request.TotalAmountMinorUnits, settings.MinimumExpenseMinorUnits))
        {
            throw new DomainException(
                InstalmentErrorCodes.ExpenseBelowThreshold,
                $"Healthcare expense must be at least {settings.MinimumExpenseMinorUnits} minor units to qualify for an instalment plan.");
        }

        var schedule = InstalmentPolicies.BuildSchedule(
            request.TotalAmountMinorUnits,
            request.InstalmentCount,
            request.Frequency,
            request.FirstDueDate);

        return Task.FromResult(InstalmentMappings.ToPreviewDto(
            request.TotalAmountMinorUnits,
            request.Frequency,
            request.InstalmentCount,
            request.Currency,
            request.FirstDueDate,
            schedule));
    }
}
