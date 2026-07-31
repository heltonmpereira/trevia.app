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
}
