namespace TreviaApp.Api.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TreviaApp.Api.Options;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Shared.Constants;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly ApiOptions _options;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IOptions<ApiOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception: {Code}", ex.ErrorCode);
            var details = new ProblemDetails
            {
                Status = GetStatusFromCode(ex.ErrorCode),
                Title = GetTitleFromCode(ex.ErrorCode),
                Detail = _options.UseDetailedErrors ? ex.Message : "Erro de domínio.",
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc9110#section-15"
            };
            if (ex.ValidationErrors != null) details.Extensions["errors"] = ex.ValidationErrors;
            context.Response.StatusCode = details.Status.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            var details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Ocorreu um erro interno.",
                Detail = _options.UseDetailedErrors ? ex.ToString() : "Erro interno do servidor.",
                Instance = context.Request.Path
            };
            context.Response.StatusCode = details.Status.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(details);
        }
    }

    private static int GetStatusFromCode(string code) => code switch
    {
        ErrorCodes.NotFound => StatusCodes.Status404NotFound,
        ErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
        ErrorCodes.ValidationError => StatusCodes.Status400BadRequest,
        ErrorCodes.InvalidCredentials => StatusCodes.Status401Unauthorized,
        ErrorCodes.EmailNotConfirmed => StatusCodes.Status403Forbidden,
        ErrorCodes.LockedOut => StatusCodes.Status423Locked,
        ErrorCodes.RefreshTokenInvalid => StatusCodes.Status401Unauthorized,
        ErrorCodes.RefreshTokenExpired => StatusCodes.Status401Unauthorized,
        ErrorCodes.DuplicateEmail => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest
    };

    private static string GetTitleFromCode(string code) => code switch
    {
        ErrorCodes.NotFound => "Recurso não encontrado",
        ErrorCodes.Unauthorized => "Não autorizado",
        ErrorCodes.Forbidden => "Acesso proibido",
        ErrorCodes.ValidationError => "Erro de validação",
        ErrorCodes.InvalidCredentials => "Credenciais inválidas",
        ErrorCodes.EmailNotConfirmed => "E-mail não confirmado",
        ErrorCodes.LockedOut => "Conta bloqueada temporariamente",
        ErrorCodes.RefreshTokenInvalid => "Refresh token inválido",
        ErrorCodes.RefreshTokenExpired => "Refresh token expirado",
        ErrorCodes.DuplicateEmail => "E-mail duplicado",
        _ => "Erro"
    };
}
