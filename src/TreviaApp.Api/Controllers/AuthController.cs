namespace TreviaApp.Api.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TreviaApp.Application.Authentication.Commands.ChangePassword;
using TreviaApp.Application.Authentication.Commands.ConfirmEmail;
using TreviaApp.Application.Authentication.Commands.ForgotPassword;
using TreviaApp.Application.Authentication.Commands.Login;
using TreviaApp.Application.Authentication.Commands.Logout;
using TreviaApp.Application.Authentication.Commands.RefreshToken;
using TreviaApp.Application.Authentication.Commands.Register;
using TreviaApp.Application.Authentication.Commands.ResendConfirmationEmail;
using TreviaApp.Application.Authentication.Commands.ResetPassword;
using TreviaApp.Application.Authentication.Commands.RevokeAllRefreshTokens;
using TreviaApp.Application.Authentication.Commands.RevokeRefreshToken;
using TreviaApp.Application.Authentication.Queries.GetActiveSessions;
using TreviaApp.Application.Authentication.Queries.GetCurrentUser;
using TreviaApp.Contracts.Authentication;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("AuthEndpoint")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    public AuthController(ISender sender) => _sender = sender;

    [HttpGet("ping")]
    [AllowAnonymous]
    [DisableRateLimiting]
    public IActionResult Ping() => Ok(new { message = "TreviaApp Auth endpoint OK", time = DateTimeOffset.UtcNow });

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var cmd = new RegisterCommand(request.Email, request.Password, request.ConfirmPassword, request.FirstName, request.LastName);
        var response = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetCurrentUser), new { }, response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var cmd = new LoginCommand(request.Email, request.Password, request.RememberMe);
        var response = await _sender.Send(cmd, ct);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var cmd = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);
        var response = await _sender.Send(cmd, ct);
        return Ok(response);
    }

    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Revoke([FromBody] RevokeRefreshTokenRequest request, CancellationToken ct)
    {
        var cmd = new RevokeRefreshTokenCommand(request.RefreshToken, "UserRevoked");
        await _sender.Send(cmd, ct);
        return NoContent();
    }

    [HttpDelete("sessions")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeAllSessions(CancellationToken ct)
    {
        await _sender.Send(new RevokeAllRefreshTokensCommand(null, "UserRevokedAll"), ct);
        return NoContent();
    }

    [HttpGet("sessions")]
    [Authorize]
    [ProducesResponseType(typeof(UserSessionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveSessions(CancellationToken ct)
    {
        var res = await _sender.Send(new GetActiveSessionsQuery(), ct);
        return Ok(res);
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken ct)
    {
        await _sender.Send(new ConfirmEmailCommand(Guid.Parse(request.UserId), request.Token), ct);
        return NoContent();
    }

    [HttpPost("resend-confirmation-email")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResendConfirmationEmail(CancellationToken ct)
    {
        await _sender.Send(new ResendConfirmationEmailCommand(), ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _sender.Send(new ForgotPasswordCommand(request.Email), ct);
        return Accepted();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await _sender.Send(new ResetPasswordCommand(Guid.Parse(request.UserId), request.Token, request.Password, request.ConfirmPassword), ct);
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _sender.Send(new ChangePasswordCommand(request.CurrentPassword, request.NewPassword, request.ConfirmNewPassword), ct);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var res = await _sender.Send(new GetCurrentUserQuery(), ct);
        return Ok(res);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _sender.Send(new LogoutCommand(), ct);
        return NoContent();
    }
}
