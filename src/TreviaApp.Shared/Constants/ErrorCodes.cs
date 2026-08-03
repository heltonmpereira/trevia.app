namespace TreviaApp.Shared.Constants;

/// <summary>
/// Provides shared constants for ErrorCodes.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Gets the constant value for Not Found.
    /// </summary>
    public const string NotFound = nameof(NotFound);
    /// <summary>
    /// Gets the constant value for Unauthorized.
    /// </summary>
    public const string Unauthorized = nameof(Unauthorized);
    /// <summary>
    /// Gets the constant value for Forbidden.
    /// </summary>
    public const string Forbidden = nameof(Forbidden);
    /// <summary>
    /// Gets the constant value for Validation Error.
    /// </summary>
    public const string ValidationError = nameof(ValidationError);
    /// <summary>
    /// Gets the constant value for Invalid Credentials.
    /// </summary>
    public const string InvalidCredentials = nameof(InvalidCredentials);
    /// <summary>
    /// Gets the constant value for Email Not Confirmed.
    /// </summary>
    public const string EmailNotConfirmed = nameof(EmailNotConfirmed);
    /// <summary>
    /// Gets the constant value for Locked Out.
    /// </summary>
    public const string LockedOut = nameof(LockedOut);
    /// <summary>
    /// Gets the constant value for Refresh Token Invalid.
    /// </summary>
    public const string RefreshTokenInvalid = nameof(RefreshTokenInvalid);
    /// <summary>
    /// Gets the constant value for Refresh Token Expired.
    /// </summary>
    public const string RefreshTokenExpired = nameof(RefreshTokenExpired);
    /// <summary>
    /// Gets the constant value for Duplicate Email.
    /// </summary>
    public const string DuplicateEmail = nameof(DuplicateEmail);
    /// <summary>
    /// Gets the constant value for Concurrency Error.
    /// </summary>
    public const string ConcurrencyError = nameof(ConcurrencyError);
    /// <summary>
    /// Gets the constant value for Profile Already Exists.
    /// </summary>
    public const string ProfileAlreadyExists = nameof(ProfileAlreadyExists);
    /// <summary>
    /// Gets the constant value for Profile Not Found.
    /// </summary>
    public const string ProfileNotFound = nameof(ProfileNotFound);
    /// <summary>
    /// Gets the constant value for Profile Private.
    /// </summary>
    public const string ProfilePrivate = nameof(ProfilePrivate);

    /// <summary>
    /// Gets the constant value for Exercise Not Found.
    /// </summary>
    public const string ExerciseNotFound = nameof(ExerciseNotFound);
    /// <summary>
    /// Gets the constant value for Exercise Not Owner.
    /// </summary>
    public const string ExerciseNotOwner = nameof(ExerciseNotOwner);
    /// <summary>
    /// Gets the constant value for Exercise Not Awaiting Approval.
    /// </summary>
    public const string ExerciseNotAwaitingApproval = nameof(ExerciseNotAwaitingApproval);
    /// <summary>
    /// Gets the constant value for Exercise Already Approved.
    /// </summary>
    public const string ExerciseAlreadyApproved = nameof(ExerciseAlreadyApproved);
    /// <summary>
    /// Gets the constant value for Exercise Already Rejected.
    /// </summary>
    public const string ExerciseAlreadyRejected = nameof(ExerciseAlreadyRejected);
    /// <summary>
    /// Gets the constant value for Exercise Reject Reason Required.
    /// </summary>
    public const string ExerciseRejectReasonRequired = nameof(ExerciseRejectReasonRequired);
    /// <summary>
    /// Gets the constant value for Exercise Slug Not Unique.
    /// </summary>
    public const string ExerciseSlugNotUnique = nameof(ExerciseSlugNotUnique);
    /// <summary>
    /// Gets the constant value for Exercise Media Not Found.
    /// </summary>
    public const string ExerciseMediaNotFound = nameof(ExerciseMediaNotFound);
    /// <summary>
    /// Gets the constant value for Muscle Already In Exercise.
    /// </summary>
    public const string MuscleAlreadyInExercise = nameof(MuscleAlreadyInExercise);
    /// <summary>
    /// Gets the constant value for Equipment Already In Exercise.
    /// </summary>
    public const string EquipmentAlreadyInExercise = nameof(EquipmentAlreadyInExercise);

    /// <summary>
    /// Gets the constant value for Training Plan Not Found.
    /// </summary>
    public const string TrainingPlanNotFound = nameof(TrainingPlanNotFound);
    /// <summary>
    /// Gets the constant value for Training Plan Not Owner.
    /// </summary>
    public const string TrainingPlanNotOwner = nameof(TrainingPlanNotOwner);
    /// <summary>
    /// Gets the constant value for Training Plan Not Editable.
    /// </summary>
    public const string TrainingPlanNotEditable = nameof(TrainingPlanNotEditable);
    /// <summary>
    /// Gets the constant value for Training Plan Not Publishable.
    /// </summary>
    public const string TrainingPlanNotPublishable = nameof(TrainingPlanNotPublishable);
    /// <summary>
    /// Gets the constant value for Training Plan Session Not Found.
    /// </summary>
    public const string TrainingPlanSessionNotFound = nameof(TrainingPlanSessionNotFound);
    /// <summary>
    /// Gets the constant value for Training Plan Session Exercise Not Found.
    /// </summary>
    public const string TrainingPlanSessionExerciseNotFound = nameof(TrainingPlanSessionExerciseNotFound);
    /// <summary>
    /// Gets the constant value for Exercise Not Approved Global.
    /// </summary>
    public const string ExerciseNotApprovedGlobal = nameof(ExerciseNotApprovedGlobal);
    /// <summary>
    /// Gets the constant value for Duplicate Exercise In Session.
    /// </summary>
    public const string DuplicateExerciseInSession = nameof(DuplicateExerciseInSession);

    /// <summary>
    /// Gets the constant value for Coach Invite Not Found.
    /// </summary>
    public const string CoachInviteNotFound = nameof(CoachInviteNotFound);
    /// <summary>
    /// Gets the constant value for Coach Link Not Found.
    /// </summary>
    public const string CoachLinkNotFound = nameof(CoachLinkNotFound);
    /// <summary>
    /// Gets the constant value for Coach Invite Not Pending.
    /// </summary>
    public const string CoachInviteNotPending = nameof(CoachInviteNotPending);
    /// <summary>
    /// Gets the constant value for Coach Invite Expired.
    /// </summary>
    public const string CoachInviteExpired = nameof(CoachInviteExpired);
    /// <summary>
    /// Gets the constant value for Coach Link Already Exists.
    /// </summary>
    public const string CoachLinkAlreadyExists = nameof(CoachLinkAlreadyExists);
    /// <summary>
    /// Gets the constant value for Coach Link Already Inactive.
    /// </summary>
    public const string CoachLinkAlreadyInactive = nameof(CoachLinkAlreadyInactive);
    /// <summary>
    /// Gets the constant value for Coach Link Not Owner Of Relationship.
    /// </summary>
    public const string CoachLinkNotOwnerOfRelationship = nameof(CoachLinkNotOwnerOfRelationship);
    /// <summary>
    /// Gets the constant value for Coach Cannot Invite Self.
    /// </summary>
    public const string CoachCannotInviteSelf = nameof(CoachCannotInviteSelf);
    /// <summary>
    /// Gets the constant value for Coach User Not Found.
    /// </summary>
    public const string CoachUserNotFound = nameof(CoachUserNotFound);
    /// <summary>
    /// Gets the constant value for Student User Not Found.
    /// </summary>
    public const string StudentUserNotFound = nameof(StudentUserNotFound);
    /// <summary>
    /// Gets the constant value for Coach Role Required.
    /// </summary>
    public const string CoachRoleRequired = nameof(CoachRoleRequired);
    /// <summary>
    /// Gets the constant value for Student Role Required.
    /// </summary>
    public const string StudentRoleRequired = nameof(StudentRoleRequired);
    /// <summary>
    /// Gets the constant value for Coach Invite Not Authorized To Respond.
    /// </summary>
    public const string CoachInviteNotAuthorizedToRespond = nameof(CoachInviteNotAuthorizedToRespond);
    /// <summary>
    /// Gets the constant value for Coach Invite Not Authorized To Cancel.
    /// </summary>
    public const string CoachInviteNotAuthorizedToCancel = nameof(CoachInviteNotAuthorizedToCancel);
    /// <summary>
    /// Gets the constant value for Coach No Active Link To Assign Plan.
    /// </summary>
    public const string CoachNoActiveLinkToAssignPlan = nameof(CoachNoActiveLinkToAssignPlan);
    /// <summary>
    /// Gets the constant value for Coach Invite Duplicate Pending.
    /// </summary>
    public const string CoachInviteDuplicatePending = nameof(CoachInviteDuplicatePending);

    /// <summary>
    /// Gets the constant value for Workout Session Not Found.
    /// </summary>
    public const string WorkoutSessionNotFound = nameof(WorkoutSessionNotFound);
    /// <summary>
    /// Gets the constant value for Workout Exercise Not Found.
    /// </summary>
    public const string WorkoutExerciseNotFound = nameof(WorkoutExerciseNotFound);
    /// <summary>
    /// Gets the constant value for Workout Set Not Found.
    /// </summary>
    public const string WorkoutSetNotFound = nameof(WorkoutSetNotFound);
    /// <summary>
    /// Gets the constant value for Workout Training Session Not Found.
    /// </summary>
    public const string WorkoutTrainingSessionNotFound = nameof(WorkoutTrainingSessionNotFound);
    /// <summary>
    /// Gets the constant value for Workout Training Plan Not Assigned To Student.
    /// </summary>
    public const string WorkoutTrainingPlanNotAssignedToStudent = nameof(WorkoutTrainingPlanNotAssignedToStudent);
    /// <summary>
    /// Gets the constant value for Workout Cannot Start Not Owner.
    /// </summary>
    public const string WorkoutCannotStartNotOwner = nameof(WorkoutCannotStartNotOwner);
    /// <summary>
    /// Gets the constant value for Workout Invalid Status Transition.
    /// </summary>
    public const string WorkoutInvalidStatusTransition = nameof(WorkoutInvalidStatusTransition);
    /// <summary>
    /// Gets the constant value for Workout Already Has Active Session.
    /// </summary>
    public const string WorkoutAlreadyHasActiveSession = nameof(WorkoutAlreadyHasActiveSession);
    /// <summary>
    /// Gets the constant value for Workout Set Already Logged.
    /// </summary>
    public const string WorkoutSetAlreadyLogged = nameof(WorkoutSetAlreadyLogged);
    /// <summary>
    /// Gets the constant value for Workout Exercise Already Skipped.
    /// </summary>
    public const string WorkoutExerciseAlreadySkipped = nameof(WorkoutExerciseAlreadySkipped);
    /// <summary>
    /// Gets the constant value for Workout Not In Progress Or Paused.
    /// </summary>
    public const string WorkoutNotInProgressOrPaused = nameof(WorkoutNotInProgressOrPaused);
    /// <summary>
    /// Gets the constant value for Workout Session Already Finished.
    /// </summary>
    public const string WorkoutSessionAlreadyFinished = nameof(WorkoutSessionAlreadyFinished);
    /// <summary>
    /// Gets the constant value for Workout Rating Invalid For Interrupted.
    /// </summary>
    public const string WorkoutRatingInvalidForInterrupted = nameof(WorkoutRatingInvalidForInterrupted);
    /// <summary>
    /// Gets the constant value for Workout Week Number Invalid.
    /// </summary>
    public const string WorkoutWeekNumberInvalid = nameof(WorkoutWeekNumberInvalid);

    /// <summary>
    /// Gets the constant value for Feedback Not Found.
    /// </summary>
    public const string FeedbackNotFound = nameof(FeedbackNotFound);
    /// <summary>
    /// Gets the constant value for Feedback Forbidden.
    /// </summary>
    public const string FeedbackForbidden = nameof(FeedbackForbidden);
    /// <summary>
    /// Gets the constant value for Feedback Cannot Send No Permission.
    /// </summary>
    public const string FeedbackCannotSendNoPermission = nameof(FeedbackCannotSendNoPermission);
    /// <summary>
    /// Gets the constant value for Feedback Text Too Long.
    /// </summary>
    public const string FeedbackTextTooLong = nameof(FeedbackTextTooLong);
    /// <summary>
    /// Gets the constant value for Feedback Empty.
    /// </summary>
    public const string FeedbackEmpty = nameof(FeedbackEmpty);

    /// <summary>
    /// Gets the constant value for Notification Not Found.
    /// </summary>
    public const string NotificationNotFound = nameof(NotificationNotFound);
    /// <summary>
    /// Gets the constant value for Notification Not Owner.
    /// </summary>
    public const string NotificationNotOwner = nameof(NotificationNotOwner);

    /// <summary>
    /// Gamification: Mission reward already claimed.
    /// </summary>
    public const string GamificationAlreadyClaimed = nameof(GamificationAlreadyClaimed);

    /// <summary>
    /// Gamification: Invalid manual points adjustment.
    /// </summary>
    public const string GamificationInvalidAdjustment = nameof(GamificationInvalidAdjustment);

    /// <summary>
    /// Gamification: Mission definition not found.
    /// </summary>
    public const string GamificationMissionNotFound = nameof(GamificationMissionNotFound);

    /// <summary>
    /// Gamification: Workout session already awarded points.
    /// </summary>
    public const string GamificationSessionAlreadyAwarded = nameof(GamificationSessionAlreadyAwarded);

    /// <summary>
    /// Gamification: Daily points cap exceeded.
    /// </summary>
    public const string GamificationDailyCapExceeded = nameof(GamificationDailyCapExceeded);

    /// <summary>
    /// Gamification: Achievement definition not found.
    /// </summary>
    public const string GamificationAchievementNotFound = nameof(GamificationAchievementNotFound);

    /// <summary>
    /// Gamification: User level not initialized.
    /// </summary>
    public const string GamificationUserLevelNotFound = nameof(GamificationUserLevelNotFound);
}
