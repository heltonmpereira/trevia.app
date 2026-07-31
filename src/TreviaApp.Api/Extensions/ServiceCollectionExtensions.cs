namespace TreviaApp.Api.Extensions;

using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using TreviaApp.Api.Options;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiOptions>(configuration);
        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(opt =>
        {
            opt.AddPolicy("AllowFrontend", builder =>
            {
                if (corsOrigins.Length == 0 || corsOrigins.Contains("*"))
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("WWW-Authenticate");
                else
                    builder.WithOrigins(corsOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials().WithExposedHeaders("WWW-Authenticate");
            });
        });

        services.AddRateLimiter(opt =>
        {
            opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opt.AddFixedWindowLimiter("FixedWindow", lim =>
            {
                lim.Window = TimeSpan.FromSeconds(30);
                lim.PermitLimit = 100;
                lim.QueueLimit = 20;
                lim.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            opt.AddFixedWindowLimiter("AuthEndpoint", lim =>
            {
                lim.Window = TimeSpan.FromMinutes(1);
                lim.PermitLimit = 10;
                lim.QueueLimit = 0;
            });
        });

        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("DefaultConnection") ?? string.Empty, name: "postgresql")
            .AddDbContextCheck<TreviaApp.Infrastructure.Persistence.ApplicationDbContext>(name: "dbcontext");

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TreviaApp API",
                Version = "v1",
                Description = "Plataforma Fitness Gamificada — API Principal",
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
                //Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                //In = ParameterLocation.Header,
                Description = "JWT Authorization header. Ex: 'Bearer {token}'"
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
