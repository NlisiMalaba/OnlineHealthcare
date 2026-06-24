using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Telemedicine.Realtime.Reconnection;

public sealed record ProcessExpiredTelemedicineReconnectionGracesCommand : ICommand<int>;
