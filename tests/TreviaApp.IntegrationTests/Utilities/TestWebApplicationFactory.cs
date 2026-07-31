namespace TreviaApp.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;
using TreviaApp.Infrastructure.Persistence;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("treviaapp_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(5432, assignRandomHostPort: true)
        .WithCleanUp(true)
        .Build();

    public string DbConnectionString => _dbContainer.GetConnectionString();
    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString())
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors());
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
        _dbConnection = context.Database.GetDbConnection();
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new Respawn.Graph.Table[]
            {
                new("__EFMigrationsHistory"),
                new("AspNetRoles"),
                new("AspNetRoleClaims")
            }
        });

        await TestingIdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
        await TestingIdentitySeeder.SeedAdminAsync(scope.ServiceProvider, email: "admin-integration@test.com", password: "AdminTest123!");
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
        using var scope = Services.CreateScope();
        await TestingIdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
        await TestingIdentitySeeder.SeedAdminAsync(scope.ServiceProvider, email: "admin-integration@test.com", password: "AdminTest123!");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
