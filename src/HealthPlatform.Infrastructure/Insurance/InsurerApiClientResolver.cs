using HealthPlatform.Application.Insurance;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Insurance;

public sealed class InsurerApiClientResolver(
    IEnumerable<IInsurerApiClient> clients) : IInsurerApiClientResolver
{
    private readonly IReadOnlyDictionary<string, IInsurerApiClient> _clients =
        clients.ToDictionary(c => c.InsurerCode, StringComparer.OrdinalIgnoreCase);

    public IInsurerApiClient GetRequired(string insurerCode)
    {
        var normalized = insurerCode.Trim().ToLowerInvariant();
        if (_clients.TryGetValue(normalized, out var client))
        {
            return client;
        }

        throw new InvalidOperationException($"Insurer API client '{insurerCode}' is not registered.");
    }
}
