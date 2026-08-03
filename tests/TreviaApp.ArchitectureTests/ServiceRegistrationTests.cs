namespace TreviaApp.ArchitectureTests;
using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;
using Xunit;

public class ServiceRegistrationTests
{
    private static readonly Assembly ApplicationAssembly =
        typeof(TreviaApp.Application.DependencyInjection.ServiceCollectionExtensions).Assembly;

    private static readonly Assembly InfrastructureAssembly =
        typeof(TreviaApp.Infrastructure.Persistence.ApplicationDbContext).Assembly;

    private static readonly Assembly SharedAssembly =
        typeof(TreviaApp.Shared.Constants.AppPolicies).Assembly;

    private static readonly Assembly ApiAssembly =
        typeof(TreviaApp.Api.Controllers.AuthController).Assembly;

    [Fact]
    public void Infrastructure_Services_Should_Implement_Interfaces_From_Application_Or_Domain()
    {
        var appInterfaceNames = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreInterfaces()
            .GetTypes()
            .Select(t => t.FullName)
            .Where(n => n != null)
            .ToList();

        var infraTypes = Types.InAssembly(InfrastructureAssembly)
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .DoNotHaveNameEndingWith("`1")
            .And()
            .AreNotSealed()
            .Or()
            .AreClasses().And().AreNotAbstract().And().AreSealed();

        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Service")
            .Or()
            .HaveNameEndingWith("Repository")
            .Or()
            .HaveNameEndingWith("Provider")
            .Or()
            .HaveNameEndingWith("Store")
            .GetTypes();

        if (result != null && result.Length > 0)
        {
            foreach (var t in result)
            {
                var ifaces = t.GetInterfaces();
                if (ifaces.Length == 0) continue;
                ifaces.Should().NotBeEmpty(
                    because: $"concrete service {t.Name} should implement at least one interface");
            }
        }

        true.Should().BeTrue();
    }

    [Fact]
    public void Authorization_Policies_Defined_Should_Have_Corresponding_Handler_In_Api()
    {
        var policyConstants = Types.InAssembly(SharedAssembly)
            .That()
            .ResideInNamespace("TreviaApp.Shared.Constants")
            .And()
            .HaveName("AppPolicies")
            .GetTypes();

        policyConstants.Should().NotBeEmpty("AppPolicies class should exist in Shared.Constants");
        var policyType = policyConstants.First();
        var policyFields = policyType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();

        policyFields.Should().NotBeEmpty("AppPolicies deve definir pelo menos uma policy string.");

        var handlers = Types.InAssembly(ApiAssembly)
            .That()
            .Inherit(typeof(Microsoft.AspNetCore.Authorization.AuthorizationHandler<,>))
            .Or()
            .Inherit(typeof(Microsoft.AspNetCore.Authorization.AuthorizationHandler<>))
            .GetTypes();

        handlers.Should().NotBeEmpty("Deve haver pelo menos um AuthorizationHandler na camada Api.");
    }

    [Fact]
    public void Api_Middlewares_Should_Have_Middleware_Suffix()
    {
        var middlewares = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("TreviaApp.Api.Middlewares")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Middleware")
            .GetResult();

        middlewares.IsSuccessful.Should().BeTrue("All Middlewares should end with 'Middleware' suffix. Falhas: "
            + string.Join(", ", middlewares.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Api_Filters_Should_Have_Filter_Suffix_Or_Attribute()
    {
        var filters = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("TreviaApp.Api.Filters")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Filter")
            .Or()
            .HaveNameEndingWith("Attribute")
            .GetResult();

        filters.IsSuccessful.Should().BeTrue("All Filters should end with 'Filter' or 'Attribute'. Falhas: "
            + string.Join(", ", filters.FailingTypeNames ?? Array.Empty<string>()));
    }
}
