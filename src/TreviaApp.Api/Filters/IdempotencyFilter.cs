namespace TreviaApp.Api.Filters;

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class IdempotencyFilter : IAsyncActionFilter
{
    private const string HeaderName = "X-Client-Request-Id";
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public IdempotencyFilter(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue(HeaderName, out var headerValues) ||
            !Guid.TryParse(headerValues.FirstOrDefault(), out var requestId))
        {
            await next();
            return;
        }

        var userId = _currentUser.UserId;
        if (userId == null || userId == Guid.Empty)
        {
            var endpointMetadata = context.ActionDescriptor.EndpointMetadata;
            var allowAnonymous = endpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
            {
                await next();
                return;
            }
            await next();
            return;
        }

        var existing = await _dbContext.Set<ProcessedClientRequest>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId.Value && r.RequestId == requestId);

        if (existing != null)
        {
            var result = new ContentResult
            {
                StatusCode = existing.StatusCode,
                Content = existing.ResponsePayload ?? string.Empty,
                ContentType = "application/json; charset=utf-8"
            };
            context.HttpContext.Response.Headers["X-Idempotent-Replayed"] = "true";
            context.Result = result;
            return;
        }

        var executedContext = await next();

        if (executedContext.Exception != null && !executedContext.ExceptionHandled)
        {
            return;
        }

        if (executedContext.Result is ObjectResult objectResult &&
            (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put || request.Method == HttpMethods.Delete))
        {
            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            var payload = objectResult.Value != null
                ? JsonSerializer.Serialize(objectResult.Value, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
                : null;

            try
            {
                _dbContext.Set<ProcessedClientRequest>().Add(new ProcessedClientRequest
                {
                    RequestId = requestId,
                    UserId = userId.Value,
                    OperationType = $"{context.ActionDescriptor.RouteValues["controller"]}/{context.ActionDescriptor.RouteValues["action"]}",
                    StatusCode = statusCode,
                    ResponsePayload = payload,
                    ProcessedAt = DateTimeOffset.UtcNow
                });
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
            }
        }
    }
}
