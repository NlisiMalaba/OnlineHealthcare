using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.MentalHealth;
using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace HealthPlatform.Infrastructure.MongoDb;

public static class MongoDbServiceCollectionExtensions
{
    public static IServiceCollection AddHealthPlatformMongoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbOptions>(configuration.GetSection(MongoDbOptions.SectionName));

        var connectionString = configuration.GetConnectionString("MongoDb")
            ?? configuration[$"{MongoDbOptions.SectionName}:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            var databaseName = configuration[$"{MongoDbOptions.SectionName}:DatabaseName"] ?? "healthplatform";
            services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
            services.AddSingleton(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
            services.AddSingleton<ITelemedicineSessionSummaryRepository, MongoTelemedicineSessionSummaryRepository>();
            services.AddSingleton<ITherapySessionSummaryRepository, MongoTherapySessionSummaryRepository>();
            services.AddSingleton<IMoodLogRepository, MongoMoodLogRepository>();
            services.AddSingleton<IAntenatalCheckupEntryRepository, MongoAntenatalCheckupEntryRepository>();
            services.AddSingleton<IHealthRecordEntryRepository, MongoHealthRecordEntryRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryTelemedicineSessionSummaryRepository>();
            services.AddSingleton<InMemoryTherapySessionSummaryRepository>();
            services.AddSingleton<InMemoryMoodLogRepository>();
            services.AddSingleton<InMemoryAntenatalCheckupEntryRepository>();
            services.AddSingleton<InMemoryHealthRecordEntryRepository>();
            services.AddSingleton<ITelemedicineSessionSummaryRepository>(sp =>
                sp.GetRequiredService<InMemoryTelemedicineSessionSummaryRepository>());
            services.AddSingleton<ITherapySessionSummaryRepository>(sp =>
                sp.GetRequiredService<InMemoryTherapySessionSummaryRepository>());
            services.AddSingleton<IMoodLogRepository>(sp =>
                sp.GetRequiredService<InMemoryMoodLogRepository>());
            services.AddSingleton<IAntenatalCheckupEntryRepository>(sp =>
                sp.GetRequiredService<InMemoryAntenatalCheckupEntryRepository>());
            services.AddSingleton<IHealthRecordEntryRepository>(sp =>
                sp.GetRequiredService<InMemoryHealthRecordEntryRepository>());
        }

        return services;
    }
}
