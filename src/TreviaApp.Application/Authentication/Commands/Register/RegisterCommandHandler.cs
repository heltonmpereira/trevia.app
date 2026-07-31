namespace TreviaApp.Application.Authentication.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TreviaApp.Application.Email;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthResponse>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenStore _refreshStore;
    private readonly IEmailSender _emailSender;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(UserManager<AppUser> userManager, IJwtTokenService jwtService, IRefreshTokenStore refreshStore, IEmailSender emailSender, ICurrentUserService currentUser, ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _refreshStore = refreshStore;
        _emailSender = emailSender;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            throw new DomainException("E-mail já cadastrado.", ErrorCodes.DuplicateEmail);

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => (object?)e.Description);
            throw new DomainException("Falha ao criar usuário.", ErrorCodes.ValidationError, errors);
        }

        var addRole = await _userManager.AddToRoleAsync(user, AppRoles.Student);
        if (!addRole.Succeeded)
            throw new DomainException("Falha ao atribuir role padrão.", ErrorCodes.ValidationError);

        _logger.LogInformation("UserRegistered UserId={UserId} Email={Email}", user.Id, user.Email);

        var claimsPrincipal = await _userManager.GetClaimsAsync(user);
        var roleClaims = (await _userManager.GetRolesAsync(user)).Select(r => new Claim(ClaimTypes.Role, r));
        var claims = claimsPrincipal.Concat(roleClaims)
            .Append(new Claim(AppClaimTypes.UserId, user.Id.ToString()))
            .Append(new Claim(ClaimTypes.Email, user.Email!));

        var auth = _jwtService.GenerateTokens(user.Id, user.Email!, claims);
        var tokenId = ExtractRefreshTokenId(auth.RefreshToken);
        var tokenHash = Hash(auth.RefreshToken);
        await _refreshStore.StoreAsync(user.Id, tokenId, tokenHash, auth.ExpiresAt.AddDays(7), _currentUser.Email ?? "api/register", GetIpAddressOrDefault(), cancellationToken);

        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedUserId = Uri.EscapeDataString(user.Id.ToString());
            var encodedToken = Uri.EscapeDataString(token);
            var confirmationLink = $"{GetClientAppBaseUrl()}/auth/confirm-email?userId={encodedUserId}&token={encodedToken}";
            await _emailSender.SendConfirmationEmailAsync(user.Email!, $"{user.FirstName} {user.LastName}".Trim(), confirmationLink, cancellationToken);
            _logger.LogInformation("ConfirmationEmailSent UserId={UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ConfirmationEmailFailed UserId={UserId}", user.Id);
        }

        return auth;
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
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private string GetIpAddressOrDefault() => _currentUser is not null && _currentUser.UserId.HasValue ? "api" : "api";

    private static string GetClientAppBaseUrl()
    {
        return "http://localhost:5005";
    }
}
