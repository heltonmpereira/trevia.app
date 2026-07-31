namespace TreviaApp.Application.Authentication.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenStore _refreshStore;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IJwtTokenService jwtService, IRefreshTokenStore refreshStore, ICurrentUserService currentUser, ILogger<LoginCommandHandler> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtService = jwtService;
        _refreshStore = refreshStore;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.IsDeleted)
        {
            _logger.LogWarning("LoginFailed Email={Email} Reason=UserNotFoundOrDeleted", request.Email);
            throw new DomainException("Credenciais inválidas.", ErrorCodes.InvalidCredentials);
        }

        var signIn = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: true);
        if (signIn.IsLockedOut)
        {
            _logger.LogWarning("LockoutTriggered UserId={UserId}", user.Id);
            throw new DomainException("Conta bloqueada temporariamente após tentativas inválidas.", ErrorCodes.LockedOut);
        }

        if (signIn.RequiresTwoFactor)
            throw new DomainException("Dois fatores requerido (não implementado nesta etapa).", ErrorCodes.ValidationError);

        if (signIn.IsNotAllowed)
            throw new DomainException("Por favor confirme seu e-mail antes de logar.", ErrorCodes.EmailNotConfirmed);

        if (!signIn.Succeeded)
        {
            _logger.LogWarning("LoginFailed UserId={UserId} Reason=BadPassword", user.Id);
            throw new DomainException("Credenciais inválidas.", ErrorCodes.InvalidCredentials);
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("LoginFailed UserId={UserId} Reason=EmailNotConfirmed", user.Id);
            throw new DomainException("Por favor confirme seu e-mail antes de logar.", ErrorCodes.EmailNotConfirmed);
        }

        user.LastActiveAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        var claims = await BuildClaims(user);
        var auth = _jwtService.GenerateTokens(user.Id, user.Email!, claims);
        var tokenId = ExtractRefreshTokenId(auth.RefreshToken);
        var tokenHash = Hash(auth.RefreshToken);
        var refreshDays = request.RememberMe ? 30 : 7;
        await _refreshStore.StoreAsync(user.Id, tokenId, tokenHash, auth.ExpiresAt.AddDays(refreshDays), "device-info", GetIp(), cancellationToken);

        _logger.LogInformation("UserLoggedIn UserId={UserId} Remember={Remember}", user.Id, request.RememberMe);
        return auth;
    }

    private async Task<List<Claim>> BuildClaims(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(AppClaimTypes.UserId, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!)
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        return claims;
    }

    private static string ExtractRefreshTokenId(string refreshToken)
    {
        var idx = refreshToken.IndexOf('.');
        return idx > 0 ? refreshToken.Substring(0, idx) : refreshToken;
    }

    private static string Hash(string s)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(s);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    private string GetIp() => "api-login";
}
