using Elastic.Clients.Elasticsearch;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Search;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Search.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Search;

public sealed class ElasticsearchSearchService(
    ElasticsearchClient client,
    IDoctorRepository doctorRepository,
    IPharmacyRepository pharmacyRepository,
    DoctorElasticsearchSearcher doctorSearcher,
    IOptions<ElasticsearchOptions> options,
    ILogger<ElasticsearchSearchService> logger) : ISearchService
{
    public async Task UpsertDoctorAsync(Guid doctorId, CancellationToken ct)
    {
        var doctor = await doctorRepository.GetByIdWithSlotsAsync(doctorId, ct);
        if (doctor is null)
        {
            logger.LogWarning("Skipping doctor search index upsert for missing doctor {DoctorId}.", doctorId);
            return;
        }

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            await RemoveDoctorAsync(doctorId, ct);
            return;
        }

        var document = DoctorSearchDocumentMapper.Map(doctor);
        var response = await client.IndexAsync(
            document,
            index => index
                .Index(options.Value.DoctorsIndex)
                .Id(doctorId.ToString()),
            ct);

        if (!response.IsValidResponse)
        {
            logger.LogError(
                "Failed to upsert doctor {DoctorId} in search index: {Error}",
                doctorId,
                response.ElasticsearchServerError?.Error.Reason ?? response.DebugInformation);
            throw new InvalidOperationException($"Failed to upsert doctor '{doctorId}' in search index.");
        }

        logger.LogInformation("Upserted doctor {DoctorId} in search index.", doctorId);
    }

    public async Task RemoveDoctorAsync(Guid doctorId, CancellationToken ct)
    {
        var response = await client.DeleteAsync(
            options.Value.DoctorsIndex,
            new Id(doctorId.ToString()),
            ct);

        if (!response.IsValidResponse && response.Result != Result.NotFound)
        {
            logger.LogError(
                "Failed to remove doctor {DoctorId} from search index: {Error}",
                doctorId,
                response.ElasticsearchServerError?.Error.Reason ?? response.DebugInformation);
            throw new InvalidOperationException($"Failed to remove doctor '{doctorId}' from search index.");
        }

        logger.LogInformation("Removed doctor {DoctorId} from search index.", doctorId);
    }

    public async Task UpsertPharmacyAsync(Guid pharmacyId, CancellationToken ct)
    {
        var pharmacy = await pharmacyRepository.GetByIdAsync(pharmacyId, ct);
        if (pharmacy is null)
        {
            logger.LogWarning("Skipping pharmacy search index upsert for missing pharmacy {PharmacyId}.", pharmacyId);
            return;
        }

        var existingStock = await TryGetExistingPharmacyStockAsync(pharmacyId, ct);
        var document = PharmacySearchDocumentMapper.Map(pharmacy, existingStock);
        var response = await client.IndexAsync(
            document,
            index => index
                .Index(options.Value.PharmaciesIndex)
                .Id(pharmacyId.ToString()),
            ct);

        if (!response.IsValidResponse)
        {
            logger.LogError(
                "Failed to upsert pharmacy {PharmacyId} in search index: {Error}",
                pharmacyId,
                response.ElasticsearchServerError?.Error.Reason ?? response.DebugInformation);
            throw new InvalidOperationException($"Failed to upsert pharmacy '{pharmacyId}' in search index.");
        }

        logger.LogInformation("Upserted pharmacy {PharmacyId} in search index.", pharmacyId);
    }

    public async Task UpdatePharmacyStockAsync(
        Guid pharmacyId,
        IReadOnlyList<PharmacyStockIndexEntry> stockSummary,
        CancellationToken ct)
    {
        var stock = PharmacySearchDocumentMapper.MapStock(stockSummary);
        var hasStock = stock.Any(entry => entry.QuantityOnHand > 0);

        var response = await client.UpdateAsync<PharmacySearchDocument, object>(
            options.Value.PharmaciesIndex,
            pharmacyId.ToString(),
            update => update.Doc(new
            {
                stockSummary = stock,
                hasStock
            }),
            ct);

        if (!response.IsValidResponse)
        {
            if (response.Result == Result.NotFound)
            {
                logger.LogWarning(
                    "Pharmacy {PharmacyId} missing from search index during stock update; upserting full document.",
                    pharmacyId);
                await UpsertPharmacyAsync(pharmacyId, ct);
                return;
            }

            logger.LogError(
                "Failed to update pharmacy {PharmacyId} stock in search index: {Error}",
                pharmacyId,
                response.ElasticsearchServerError?.Error.Reason ?? response.DebugInformation);
            throw new InvalidOperationException($"Failed to update pharmacy '{pharmacyId}' stock in search index.");
        }

        logger.LogInformation(
            "Updated pharmacy {PharmacyId} stock in search index with {ItemCount} items.",
            pharmacyId,
            stockSummary.Count);
    }

    private async Task<IReadOnlyList<PharmacyStockIndexEntry>> TryGetExistingPharmacyStockAsync(
        Guid pharmacyId,
        CancellationToken ct)
    {
        var response = await client.GetAsync<PharmacySearchDocument>(
            options.Value.PharmaciesIndex,
            pharmacyId.ToString(),
            ct);

        if (!response.Found || response.Source?.StockSummary is null)
        {
            return [];
        }

        return response.Source.StockSummary
            .Select(entry => new PharmacyStockIndexEntry(
                entry.MedicationName,
                entry.MedicationSku,
                entry.QuantityOnHand))
            .ToList();
    }

    public Task<DoctorSearchPageDto> SearchDoctorsAsync(DoctorSearchCriteria criteria, CancellationToken ct) =>
        doctorSearcher.SearchAsync(criteria, ct);
}
