namespace TreviaApp.Shared.Constants;

public static class ErrorCodes
{
    public const string NotFound = nameof(NotFound);
    public const string Unauthorized = nameof(Unauthorized);
    public const string Forbidden = nameof(Forbidden);
    public const string ValidationError = nameof(ValidationError);
    public const string InvalidCredentials = nameof(InvalidCredentials);
    public const string EmailNotConfirmed = nameof(EmailNotConfirmed);
    public const string LockedOut = nameof(LockedOut);
    public const string RefreshTokenInvalid = nameof(RefreshTokenInvalid);
    public const string RefreshTokenExpired = nameof(RefreshTokenExpired);
    public const string DuplicateEmail = nameof(DuplicateEmail);
    public const string ConcurrencyError = nameof(ConcurrencyError);
    public const string ProfileAlreadyExists = nameof(ProfileAlreadyExists);
    public const string ProfileNotFound = nameof(ProfileNotFound);
    public const string ProfilePrivate = nameof(ProfilePrivate);

    public const string ExerciseNotFound = nameof(ExerciseNotFound);
    public const string ExerciseNotOwner = nameof(ExerciseNotOwner);
    public const string ExerciseNotAwaitingApproval = nameof(ExerciseNotAwaitingApproval);
    public const string ExerciseAlreadyApproved = nameof(ExerciseAlreadyApproved);
    public const string ExerciseAlreadyRejected = nameof(ExerciseAlreadyRejected);
    public const string ExerciseRejectReasonRequired = nameof(ExerciseRejectReasonRequired);
    public const string ExerciseSlugNotUnique = nameof(ExerciseSlugNotUnique);
    public const string ExerciseMediaNotFound = nameof(ExerciseMediaNotFound);
    public const string MuscleAlreadyInExercise = nameof(MuscleAlreadyInExercise);
    public const string EquipmentAlreadyInExercise = nameof(EquipmentAlreadyInExercise);

    public const string TrainingPlanNotFound = nameof(TrainingPlanNotFound);
    public const string TrainingPlanNotOwner = nameof(TrainingPlanNotOwner);
    public const string TrainingPlanNotEditable = nameof(TrainingPlanNotEditable);
    public const string TrainingPlanNotPublishable = nameof(TrainingPlanNotPublishable);
    public const string TrainingPlanSessionNotFound = nameof(TrainingPlanSessionNotFound);
    public const string TrainingPlanSessionExerciseNotFound = nameof(TrainingPlanSessionExerciseNotFound);
    public const string ExerciseNotApprovedGlobal = nameof(ExerciseNotApprovedGlobal);
    public const string DuplicateExerciseInSession = nameof(DuplicateExerciseInSession);

    public const string CoachInviteNotFound = nameof(CoachInviteNotFound);
    public const string CoachLinkNotFound = nameof(CoachLinkNotFound);
    public const string CoachInviteNotPending = nameof(CoachInviteNotPending);
    public const string CoachInviteExpired = nameof(CoachInviteExpired);
    public const string CoachLinkAlreadyExists = nameof(CoachLinkAlreadyExists);
    public const string CoachLinkAlreadyInactive = nameof(CoachLinkAlreadyInactive);
    public const string CoachLinkNotOwnerOfRelationship = nameof(CoachLinkNotOwnerOfRelationship);
    public const string CoachCannotInviteSelf = nameof(CoachCannotInviteSelf);
    public const string CoachUserNotFound = nameof(CoachUserNotFound);
    public const string StudentUserNotFound = nameof(StudentUserNotFound);
    public const string CoachRoleRequired = nameof(CoachRoleRequired);
    public const string StudentRoleRequired = nameof(StudentRoleRequired);
    public const string CoachInviteNotAuthorizedToRespond = nameof(CoachInviteNotAuthorizedToRespond);
    public const string CoachInviteNotAuthorizedToCancel = nameof(CoachInviteNotAuthorizedToCancel);
    public const string CoachNoActiveLinkToAssignPlan = nameof(CoachNoActiveLinkToAssignPlan);
    public const string CoachInviteDuplicatePending = nameof(CoachInviteDuplicatePending);

    public const string WorkoutSessionNotFound = nameof(WorkoutSessionNotFound);
    public const string WorkoutExerciseNotFound = nameof(WorkoutExerciseNotFound);
    public const string WorkoutSetNotFound = nameof(WorkoutSetNotFound);
    public const string WorkoutTrainingSessionNotFound = nameof(WorkoutTrainingSessionNotFound);
    public const string WorkoutTrainingPlanNotAssignedToStudent = nameof(WorkoutTrainingPlanNotAssignedToStudent);
    public const string WorkoutCannotStartNotOwner = nameof(WorkoutCannotStartNotOwner);
    public const string WorkoutInvalidStatusTransition = nameof(WorkoutInvalidStatusTransition);
    public const string WorkoutAlreadyHasActiveSession = nameof(WorkoutAlreadyHasActiveSession);
    public const string WorkoutSetAlreadyLogged = nameof(WorkoutSetAlreadyLogged);
    public const string WorkoutExerciseAlreadySkipped = nameof(WorkoutExerciseAlreadySkipped);
    public const string WorkoutNotInProgressOrPaused = nameof(WorkoutNotInProgressOrPaused);
    public const string WorkoutSessionAlreadyFinished = nameof(WorkoutSessionAlreadyFinished);
    public const string WorkoutRatingInvalidForInterrupted = nameof(WorkoutRatingInvalidForInterrupted);
    public const string WorkoutWeekNumberInvalid = nameof(WorkoutWeekNumberInvalid);
}
