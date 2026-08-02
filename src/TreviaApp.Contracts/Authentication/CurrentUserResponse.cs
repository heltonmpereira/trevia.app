namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the CurrentUserResponse contract.
/// </summary>
/// <param name="UserId">User Id value.</param>
/// <param name="Email">Email value.</param>
/// <param name="EmailConfirmed">Email Confirmed value.</param>
/// <param name="FirstName">First Name value.</param>
/// <param name="LastName">Last Name value.</param>
/// <param name="DisplayName">Display Name value.</param>
/// <param name="CreatedAt">Created At value.</param>
/// <param name="LastActiveAt">Last Active At value.</param>
/// <param name="Roles">Roles value.</param>
public record CurrentUserResponse(
    Guid UserId,
    string Email,
    bool EmailConfirmed,
    string FirstName,
    string LastName,
    string? DisplayName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastActiveAt,
    IReadOnlyList<string> Roles);
