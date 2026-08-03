using Serilog;
using TreviaApp.Api.Extensions;
using TreviaApp.Api.Middlewares;
using TreviaApp.Infrastructure.DependencyInjection;
using TreviaApp.Infrastructure.Persistence.Seeder;
using TreviaApp.Application.DependencyInjection;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", isEnabled: true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", isEnabled: false);

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
    .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddApiAuthorizationPolicies();

var isMigrateOnly = args.Any(a =>
    a.Equals("--migrate-only", StringComparison.OrdinalIgnoreCase) ||
    a.Equals("/migrate-only", StringComparison.OrdinalIgnoreCase));

var app = builder.Build();

if (isMigrateOnly)
{
    Log.Information("Running in --migrate-only mode: applying migrations then exiting...");
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider
        .GetRequiredService<TreviaApp.Infrastructure.Persistence.ApplicationDbContext>();
    await db.Database.MigrateAsync();
    Log.Information("Migrations applied successfully.");
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAllAsync();
    Log.Information("Seeding completed. Exiting.");
    await app.StopAsync();
    return;
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.UseCors("AllowFrontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers()
   .RequireRateLimiting(TreviaApp.Shared.Constants.RateLimitPolicyNames.FixedWindowDefault);

app.MapCustomHealthChecks();
app.UseSwaggerUi();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAllAsync();
}

Log.Information("TreviaApp API started in {Env} mode.", app.Environment.EnvironmentName);
app.Run();

public partial class Program { }
