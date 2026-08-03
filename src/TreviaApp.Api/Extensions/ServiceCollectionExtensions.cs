namespace TreviaApp.Api.Extensions;

using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using TreviaApp.Api.Filters;
using TreviaApp.Api.HealthChecks;
using TreviaApp.Api.Options;
using TreviaApp.Shared.Constants;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiOptions>(configuration);

        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        var isDevelopment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";

        services.AddCors(opt =>
        {
            opt.AddPolicy("AllowFrontend", builder =>
            {
                bool hasWildcard = corsOrigins.Length == 0 || corsOrigins.Contains("*");
                if (isDevelopment && hasWildcard)
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
                           .WithExposedHeaders("WWW-Authenticate", "X-Idempotent-Replayed", "X-Client-Request-Id");
                }
                else if (!hasWildcard)
                {
                    builder.WithOrigins(corsOrigins)
                           .AllowAnyMethod().AllowAnyHeader().AllowCredentials()
                           .WithExposedHeaders("WWW-Authenticate", "X-Idempotent-Replayed", "X-Client-Request-Id")
                           .SetIsOriginAllowedToAllowWildcardSubdomains();
                }
                else
                {
                    if (!isDevelopment)
                    {
                        throw new InvalidOperationException(
                            "Production CORS requires explicit Cors:AllowedOrigins configuration. Wildcard '*' is not allowed in Production for security.");
                    }
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
                           .WithExposedHeaders("WWW-Authenticate", "X-Idempotent-Replayed");
                }
            });
        });

        services.AddRateLimiter(opt =>
        {
            opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opt.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";
                const int retryAfterSeconds = 30;
                context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://datatracker.ietf.org/doc/html/rfc6585#section-4",
                    title = "Too Many Requests",
                    status = 429,
                    detail = "Rate limit exceeded. Please retry later.",
                    retry_after_seconds = retryAfterSeconds
                }, token);
            };

            opt.AddFixedWindowLimiter(RateLimitPolicyNames.FixedWindowDefault, lim =>
            {
                lim.Window = TimeSpan.FromSeconds(30);
                lim.PermitLimit = 100;
                lim.QueueLimit = 20;
                lim.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            opt.AddFixedWindowLimiter(RateLimitPolicyNames.AuthEndpoint, lim =>
            {
                lim.Window = TimeSpan.FromMinutes(1);
                lim.PermitLimit = 5;
                lim.QueueLimit = 0;
            });

            opt.AddTokenBucketLimiter(RateLimitPolicyNames.WorkoutWrite, lim =>
            {
                lim.TokenLimit = 240;
                lim.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                lim.QueueLimit = 30;
                lim.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
                lim.TokensPerPeriod = 20;
                lim.AutoReplenishment = true;
            });

            opt.AddFixedWindowLimiter(RateLimitPolicyNames.ReadEndpoint, lim =>
            {
                lim.Window = TimeSpan.FromMinutes(1);
                lim.PermitLimit = 300;
                lim.QueueLimit = 50;
            });

            opt.AddFixedWindowLimiter(RateLimitPolicyNames.AdminEndpoint, lim =>
            {
                lim.Window = TimeSpan.FromMinutes(1);
                lim.PermitLimit = 60;
                lim.QueueLimit = 10;
            });
        });

        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "ready", "live", "db" })
            .AddDbContextCheck<TreviaApp.Infrastructure.Persistence.ApplicationDbContext>(
                name: "dbcontext", tags: new[] { "ready", "db" })
            .AddCheck<DiskStorageHealthCheck>(
                "disk_storage", tags: new[] { "ready", "infra" },
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded)
            .AddCheck<MemoryHealthCheck>(
                "memory_usage", tags: new[] { "live", "infra" },
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded);

        services.AddScoped<IdempotencyFilter>();

        services.AddControllers(options =>
        {
            options.Filters.AddService(typeof(IdempotencyFilter), order: 10);
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TreviaApp API",
                Version = "v1",
                Description = "Plataforma Fitness Gamificada — API Principal (Sprint 12 Beta)",
                Contact = new OpenApiContact { Name = "TreviaApp" }
            });
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
            var sharedXml = Path.Combine(AppContext.BaseDirectory, "TreviaApp.Domain.xml");
            if (File.Exists(sharedXml)) c.IncludeXmlComments(sharedXml);
            var contractsXml = Path.Combine(AppContext.BaseDirectory, "TreviaApp.Contracts.xml");
            if (File.Exists(contractsXml)) c.IncludeXmlComments(contractsXml);
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header. Ex: 'Bearer {token}'. Header opcional X-Client-Request-Id para idempotência de POST/PUT/DELETE."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
            c.EnableAnnotations();
        });
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        return services;
    }
}
