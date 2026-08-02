namespace TreviaApp.Domain.Errors;

/// <summary>
/// Represents the DomainErrors domain entity.
/// </summary>
public static class DomainErrors
{
    /// <summary>
    /// Represents the General domain entity.
    /// </summary>
    public static class General
    {
        /// <summary>
        /// Indicates that the requested resource was not found.
        /// </summary>
        public const string NotFound = "Resource not found.";

        /// <summary>
        /// Indicates that the current user is not authenticated or authorized.
        /// </summary>
        public const string Unauthorized = "User is not authorized.";

        /// <summary>
        /// Indicates that the current user does not have permission to perform the action.
        /// </summary>
        public const string Forbidden = "User does not have permission to perform this action.";

        /// <summary>
        /// Indicates that one or more validation rules failed.
        /// </summary>
        public const string ValidationError = "Validation failed.";

        /// <summary>
        /// Indicates that a concurrency conflict occurred while persisting changes.
        /// </summary>
        public const string ConcurrencyError = "A concurrency conflict occurred.";
    }

    /// <summary>
    /// Represents the Identity domain entity.
    /// </summary>
    public static class Identity
    {
        /// <summary>
        /// Indicates that the supplied credentials are invalid.
        /// </summary>
        public const string InvalidCredentials = "Invalid email or password.";

        /// <summary>
        /// Indicates that the user email has not been confirmed yet.
        /// </summary>
        public const string EmailNotConfirmed = "Email is not confirmed.";

        /// <summary>
        /// Indicates that the user account is currently locked out.
        /// </summary>
        public const string LockedOut = "User account is locked out.";

        /// <summary>
        /// Indicates that the supplied refresh token is invalid.
        /// </summary>
        public const string RefreshTokenInvalid = "Refresh token is invalid.";

        /// <summary>
        /// Indicates that the supplied refresh token has expired.
        /// </summary>
        public const string RefreshTokenExpired = "Refresh token has expired.";

        /// <summary>
        /// Indicates that a user with the same email already exists.
        /// </summary>
        public const string DuplicateEmail = "A user with this email already exists.";
    }
}
