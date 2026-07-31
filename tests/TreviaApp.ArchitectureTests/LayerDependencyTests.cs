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
    public void Application_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(JoinFailures(result));
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

    private static string JoinFailures(TestResult result)
    {
        if (result.IsSuccessful) return string.Empty;
        return "Falhas: " + string.Join(", ", result.FailingTypeNames ?? new string[0]);
    }
}
