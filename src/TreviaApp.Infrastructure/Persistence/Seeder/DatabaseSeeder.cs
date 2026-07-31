namespace TreviaApp.Infrastructure.Persistence.Seeder;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TreviaApp.Infrastructure.Identity;
using TreviaApp.Shared.Constants;

public class DatabaseSeeder
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly AdminSeedOptions _adminOptions;

    public DatabaseSeeder(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IOptions<AdminSeedOptions> adminOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _adminOptions = adminOptions.Value;
    }

    public async Task SeedAllAsync()
    {
        await SeedRolesAsync();
        await SeedAdminAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new (string Name, string Desc)[]
        {
            (AppRoles.Administrator, "Acesso total ao sistema"),
            (AppRoles.Student, "Aluno que realiza treinos"),
            (AppRoles.Trainer, "Professor que cria fichas e acompanha alunos"),
            (AppRoles.GymManager, "Gestor de academia")
        };

        foreach (var (name, desc) in roles)
        {
            if (!await _roleManager.RoleExistsAsync(name))
                await _roleManager.CreateAsync(new AppRole(name) { Description = desc });
        }
    }

    private async Task SeedAdminAsync()
    {
        if (string.IsNullOrWhiteSpace(_adminOptions.Email) || string.IsNullOrWhiteSpace(_adminOptions.Password))
            return;

        if (await _userManager.FindByEmailAsync(_adminOptions.Email) != null)
            return;

        var admin = new AppUser
        {
            UserName = _adminOptions.Email,
            Email = _adminOptions.Email,
            FirstName = "Administrador",
            LastName = "Sistema",
            DisplayName = "Admin",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(admin, _adminOptions.Password);
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(admin, AppRoles.Administrator);
    }
}

public class AdminSeedOptions
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
