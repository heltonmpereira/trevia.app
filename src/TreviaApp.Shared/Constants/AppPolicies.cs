namespace TreviaApp.Shared.Constants;

public static class AppPolicies
{
    public const string IsAdmin = nameof(IsAdmin);
    public const string IsTrainer = nameof(IsTrainer);
    public const string IsStudent = nameof(IsStudent);
    public const string IsTrainerOrAdmin = nameof(IsTrainerOrAdmin);
    public const string IsGymManagerOrAdmin = nameof(IsGymManagerOrAdmin);
    public const string CanManageUsers = nameof(CanManageUsers);
    public const string CanManageExercises = nameof(CanManageExercises);
    public const string CanCreateTrainingPlans = nameof(CanCreateTrainingPlans);
    public const string AuthenticatedUser = nameof(AuthenticatedUser);
    public const string IsProfileOwner = nameof(IsProfileOwner);
    public const string IsLinkedTrainer = nameof(IsLinkedTrainer);
    public const string CanManageConsents = nameof(CanManageConsents);
    public const string CanModerateExercises = nameof(CanModerateExercises);
    public const string IsExerciseOwner = nameof(IsExerciseOwner);
}
