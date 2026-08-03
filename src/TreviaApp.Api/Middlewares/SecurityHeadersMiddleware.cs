namespace TreviaApp.Api.Middlewares;

using Microsoft.AspNetCore.Http;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var response = context.Response;

        if (!response.Headers.ContainsKey("X-Content-Type-Options"))
            response.Headers["X-Content-Type-Options"] = "nosniff";

        if (!response.Headers.ContainsKey("X-Frame-Options"))
            response.Headers["X-Frame-Options"] = "DENY";

        if (!response.Headers.ContainsKey("Referrer-Policy"))
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        if (!response.Headers.ContainsKey("Permissions-Policy"))
            response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=(), bluetooth=()";

        if (!response.Headers.ContainsKey("X-XSS-Protection"))
            response.Headers["X-XSS-Protection"] = "1; mode=block";

        if (!_env.IsDevelopment())
        {
            if (!response.Headers.ContainsKey("Strict-Transport-Security"))
                response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            if (!response.Headers.ContainsKey("Content-Security-Policy"))
            {
                response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                    "img-src 'self' data: https: blob:; " +
                    "font-src 'self' data: https://fonts.gstatic.com; " +
                    "connect-src 'self' ws: wss: https:; " +
                    "frame-ancestors 'none'; " +
                    "form-action 'self'; " +
                    "base-uri 'self'; " +
                    "object-src 'none'; " +
                    "upgrade-insecure-requests";
            }
        }
        else
        {
            if (!response.Headers.ContainsKey("Content-Security-Policy-Report-Only"))
            {
                response.Headers["Content-Security-Policy-Report-Only"] =
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' localhost:* ws://localhost:*; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                    "img-src 'self' data: https: blob:; " +
                    "font-src 'self' data: https://fonts.gstatic.com; " +
                    "connect-src 'self' ws: wss: https: http: localhost:*; " +
                    "frame-ancestors 'none'; " +
                    "object-src 'none'";
            }
        }

        await _next(context);
    }
}
