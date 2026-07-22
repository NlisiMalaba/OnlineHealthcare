using HealthPlatform.API.Requests.Wellness;
using HealthPlatform.Application.Wellness.HealthGoals.CreateHealthGoal;
using HealthPlatform.Application.Wellness.HealthGoals.UpdateHealthGoal;
using HealthPlatform.Application.Wellness.WellnessEntries.RecordWellnessEntry;

namespace HealthPlatform.API.Mapping;

public static class WellnessCommandMapper
{
    public static CreateHealthGoalCommand ToCreateGoalCommand(CreateHealthGoalRequest request) =>
        new(request.MetricType, request.TargetValue, request.Unit, request.CustomLabel);

    public static UpdateHealthGoalCommand ToUpdateGoalCommand(Guid goalId, UpdateHealthGoalRequest request) =>
        new(goalId, request.TargetValue, request.Unit, request.CustomLabel);

    public static RecordWellnessEntryCommand ToRecordEntryCommand(RecordWellnessEntryRequest request) =>
        new(request.MetricType, request.Value, request.GoalId, request.RecordedAtUtc);
}
