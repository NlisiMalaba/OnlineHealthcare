using HealthPlatform.API.Requests.Wellness;
using HealthPlatform.Application.Wellness.CarePlans;
using HealthPlatform.Application.Wellness.CarePlans.AssignCarePlan;
using HealthPlatform.Application.Wellness.CarePlans.UpdateCarePlan;
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

    public static AssignCarePlanCommand ToAssignCarePlanCommand(AssignCarePlanRequest request) =>
        new(
            request.PatientId,
            request.Condition,
            request.Tasks.Select(ToTaskInput).ToList(),
            request.MonitoringTargets.Select(ToTargetInput).ToList(),
            request.ReviewIntervalDays);

    public static UpdateCarePlanCommand ToUpdateCarePlanCommand(Guid carePlanId, UpdateCarePlanRequest request) =>
        new(
            carePlanId,
            request.Condition,
            request.Tasks.Select(ToTaskInput).ToList(),
            request.MonitoringTargets.Select(ToTargetInput).ToList(),
            request.ReviewIntervalDays,
            request.NextReviewAt);

    private static CarePlanTaskInput ToTaskInput(CarePlanTaskRequest request) =>
        new(request.Id, request.Title, request.Description, request.DueDate);

    private static CarePlanMonitoringTargetInput ToTargetInput(CarePlanMonitoringTargetRequest request) =>
        new(request.MetricName, request.TargetValue, request.Unit);
}
