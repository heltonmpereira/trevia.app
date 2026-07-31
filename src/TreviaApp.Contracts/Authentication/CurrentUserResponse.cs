namespace TreviaApp.Contracts.Authentication;

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
