namespace TreviaApp.Application.Authentication.Commands.RevokeRefreshToken;
using FluentValidation;

public class RevokeRefreshTokenCommandValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}
