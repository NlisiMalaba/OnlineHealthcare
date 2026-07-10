namespace HealthPlatform.Domain.MentalHealth;

public sealed class TherapySessionCompletionNotAllowedException(TherapySessionStatus status)
    : Exception($"Therapy session cannot be completed while status is '{status}'.");
