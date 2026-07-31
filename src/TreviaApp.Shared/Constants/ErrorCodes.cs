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
}
