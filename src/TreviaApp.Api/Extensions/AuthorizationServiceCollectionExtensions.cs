namespace TreviaApp.Api.Extensions;

using Microsoft.AspNetCore.Authorization;
using TreviaApp.Api.Extensions.Authorization;
using TreviaApp.Shared.Constants;

public static class AuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddApiAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AppPolicies.IsAdmin, policy => policy.RequireRole(AppRoles.Administrator))
            .AddPolicy(AppPolicies.IsTrainer, policy => policy.RequireRole(AppRoles.Trainer))
            .AddPolicy(AppPolicies.IsStudent, policy => policy.RequireRole(AppRoles.Student))
            .AddPolicy(AppPolicies.IsTrainerOrAdmin, policy => policy.RequireRole(AppRoles.Trainer, AppRoles.Administrator))
            .AddPolicy(AppPolicies.IsGymManagerOrAdmin, policy => policy.RequireRole(AppRoles.GymManager, AppRoles.Administrator))
            .AddPolicy(AppPolicies.CanManageUsers, policy => policy.RequireRole(AppRoles.Administrator, AppRoles.GymManager))
            .AddPolicy(AppPolicies.CanManageExercises, policy => policy.RequireRole(AppRoles.Trainer, AppRoles.Administrator))
            .AddPolicy(AppPolicies.CanCreateTrainingPlans, policy => policy.RequireRole(AppRoles.Trainer, AppRoles.Administrator))
            .AddPolicy(AppPolicies.AuthenticatedUser, policy => policy.RequireAuthenticatedUser())
            .AddPolicy(AppPolicies.CanManageConsents, policy =>
                policy.RequireRole(AppRoles.Administrator, AppRoles.GymManager))
            .AddPolicy(AppPolicies.IsProfileOwner, policy =>
                policy.AddRequirements(new IsProfileOwnerRequirement()));

        services.Configure<AuthorizationOptions>(opts =>
        {
            opts.InvokeHandlersAfterFailure = false;
        });

        services.AddScoped<IAuthorizationHandler, ProfileOwnerAuthorizationHandler>();

        return services;
    }
}
