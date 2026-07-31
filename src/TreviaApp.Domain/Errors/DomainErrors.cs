namespace TreviaApp.Domain.Errors;

public static class DomainErrors
{
    public static class General
    {
        public const string NotFound = "Resource not found.";
        public const string Unauthorized = "User is not authorized.";
        public const string Forbidden = "User does not have permission to perform this action.";
        public const string ValidationError = "Validation failed.";
        public const string ConcurrencyError = "A concurrency conflict occurred.";
    }

    public static class Identity
    {
        public const string InvalidCredentials = "Invalid email or password.";
        public const string EmailNotConfirmed = "Email is not confirmed.";
        public const string LockedOut = "User account is locked out.";
        public const string RefreshTokenInvalid = "Refresh token is invalid.";
        public const string RefreshTokenExpired = "Refresh token has expired.";
        public const string DuplicateEmail = "A user with this email already exists.";
    }
}
