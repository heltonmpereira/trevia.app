namespace TreviaApp.ArchitectureTests;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

public class LayerDependencyTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(TreviaApp.Domain.Abstractions.Entity).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(TreviaApp.Application.DependencyInjection.ServiceCollectionExtensions).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(TreviaApp.Infrastructure.Persistence.ApplicationDbContext).Assembly;
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(TreviaApp.Api.Controllers.AuthController).Assembly;
    private static readonly System.Reflection.Assembly SharedAssembly = typeof(TreviaApp.Shared.Constants.AppPolicies).Assembly;
    private static readonly System.Reflection.Assembly ContractsAssembly = typeof(TreviaApp.Contracts.Authentication.AuthResponse).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(JoinFailures(result));
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(JoinFailures(result));
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationAssembly.GetName().Name)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(JoinFailures(result));
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Contracts()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ContractsAssembly.GetName().Name)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(JoinFailures(result));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(JoinFailures(result));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Directly()
    {
        var appNotInfra = Types.InAssembly(ApplicationAssembly)
            .That()
            .DoNotResideInNamespace("TreviaApp.Application.DependencyInjection")
            .ShouldNot()
            .HaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();
        appNotInfra.IsSuccessful.Should().BeTrue(JoinFailures(appNotInfra)
            + " (apenas DependencyInjection de Application pode configurar Infrastructure)");
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(JoinFailures(result));
    }

    [Fact]
    public void Api_Should_Not_Depend_On_Domain_Directly_Except_Startup()
    {
        var controllers = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOn(DomainAssembly.GetName().Name)
            .GetResult();
        controllers.IsSuccessful.Should().BeTrue(JoinFailures(controllers) +
            " (Controllers devem usar Contracts/DTOs, não referenciar Domain)");
    }

    [Fact]
    public void Api_Controllers_Should_Reside_In_Controllers_Namespace_And_Have_Suffix()
    {
        var controllers = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespaceMatching("TreviaApp\\.Api\\.Controllers.*")
            .And()
            .AreNotAbstract()
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();
        controllers.IsSuccessful.Should().BeTrue(JoinFailures(controllers));
    }

    [Fact]
    public void Controllers_Should_Not_Directly_Use_DbContext()
    {
        var forbidden = new[]
        {
            "TreviaApp.Infrastructure.Persistence.ApplicationDbContext",
            "Microsoft.EntityFrameworkCore.DbContext",
            "Microsoft.EntityFrameworkCore.DbSet`1"
        };
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(JoinFailures(result));
    }

    [Fact]
    public void Domain_Entities_Should_Be_In_Domain_Namespace()
    {
        var entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(TreviaApp.Domain.Abstractions.Entity))
            .Or()
            .Inherit(typeof(TreviaApp.Domain.Abstractions.AggregateRoot))
            .Should()
            .ResideInNamespaceStartingWith("TreviaApp.Domain")
            .GetResult();
        entityTypes.IsSuccessful.Should().BeTrue(JoinFailures(entityTypes));
    }

    [Fact]
    public void Contracts_Should_Only_Depend_On_Shared()
    {
        var forbidden = new[]
        {
            DomainAssembly.GetName().Name!,
            ApplicationAssembly.GetName().Name!,
            InfrastructureAssembly.GetName().Name!,
            ApiAssembly.GetName().Name!
        };

        var result = Types.InAssembly(ContractsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(JoinFailures(result) +
            " (Contracts só deve depender de Shared)");
    }

    [Fact]
    public void Shared_Should_Not_Depend_On_Any_Internal_Layer()
    {
        var forbidden = new[]
        {
            DomainAssembly.GetName().Name!,
            ApplicationAssembly.GetName().Name!,
            InfrastructureAssembly.GetName().Name!,
            ApiAssembly.GetName().Name!,
            ContractsAssembly.GetName().Name!
        };
        var result = Types.InAssembly(SharedAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(JoinFailures(result) +
            " (Shared é a base, não deve depender de outras camadas internas)");
    }

    private static string JoinFailures(TestResult result)
    {
        if (result.IsSuccessful) return string.Empty;
        return "Falhas: " + string.Join(", ", result.FailingTypeNames ?? new string[0]);
    }
}
