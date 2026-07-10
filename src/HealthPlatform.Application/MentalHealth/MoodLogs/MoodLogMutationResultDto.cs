using HealthPlatform.Application.MentalHealth.CrisisProtocol;

namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public sealed record MoodLogMutationResultDto(
    MoodLogDto MoodLog,
    CrisisProtocolDto CrisisProtocol);
