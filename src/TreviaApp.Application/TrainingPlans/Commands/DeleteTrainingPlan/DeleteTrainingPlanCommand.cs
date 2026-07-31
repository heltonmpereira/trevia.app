namespace TreviaApp.Application.TrainingPlans.Commands.DeleteTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;

public sealed record DeleteTrainingPlanCommand(Guid TrainingPlanId) : ICommand;
