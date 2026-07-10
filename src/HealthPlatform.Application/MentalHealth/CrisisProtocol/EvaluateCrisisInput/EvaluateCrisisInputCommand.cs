using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.MentalHealth.CrisisProtocol;

namespace HealthPlatform.Application.MentalHealth.CrisisProtocol.EvaluateCrisisInput;

public sealed record EvaluateCrisisInputCommand(string InputText) : ICommand<CrisisProtocolDto>;
