namespace TreviaApp.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                      ?? "User Id=postgres.gsspifcikscjtuswqulh;Password=kv2KVIUU5q25wrqO;Server=aws-1-us-west-2.pooler.supabase.com;Port=5432;Database=postgres";

        optionsBuilder.UseNpgsql(connStr, o => o.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
