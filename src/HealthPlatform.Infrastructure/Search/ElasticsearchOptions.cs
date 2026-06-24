namespace HealthPlatform.Infrastructure.Search;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Uri { get; set; } = string.Empty;

    public string DoctorsIndex { get; set; } = "hp-doctors";

    public string PharmaciesIndex { get; set; } = "hp-pharmacies";

    public string LabPartnersIndex { get; set; } = "hp-lab-partners";

    public bool EnsureIndicesOnStartup { get; set; } = true;
}
