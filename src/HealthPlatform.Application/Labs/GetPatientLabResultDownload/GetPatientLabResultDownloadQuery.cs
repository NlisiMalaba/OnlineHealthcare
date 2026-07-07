using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Labs.GetPatientLabResultDownload;

public sealed record GetPatientLabResultDownloadQuery(Guid LabResultId) : IQuery<LabResultDownloadDto>;
