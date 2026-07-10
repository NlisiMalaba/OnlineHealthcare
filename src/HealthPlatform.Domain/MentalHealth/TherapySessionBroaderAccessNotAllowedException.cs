namespace HealthPlatform.Domain.MentalHealth;

public sealed class TherapySessionBroaderAccessNotAllowedException(TherapySessionStatus status)
    : Exception($"Broader access cannot be granted while therapy session status is '{status}'.");
