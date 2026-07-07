using HealthPlatform.Application.HealthRecords;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Infrastructure.HealthRecords;

public static class HealthRecordsServiceCollectionExtensions
{
    public static IServiceCollection AddHealthRecordServices(this IServiceCollection services)
    {
        services.AddSingleton<IHealthRecordPdfGenerator, QuestPdfHealthRecordPdfGenerator>();
        return services;
    }
}
