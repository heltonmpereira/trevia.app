namespace TreviaApp.ArchitectureTests;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

public class ControllerRulesTests
{
    private static readonly System.Reflection.Assembly ApiAssembly =
        typeof(TreviaApp.Api.Controllers.AuthController).Assembly;

    private static readonly System.Reflection.Assembly DomainAssembly =
        typeof(TreviaApp.Domain.Abstractions.Entity).Assembly;

    private static readonly System.Reflection.Assembly InfrastructureAssembly =
        typeof(TreviaApp.Infrastructure.Persistence.ApplicationDbContext).Assembly;

    [Fact]
    public void Controllers_Should_InheritFrom_ApiControllerBase()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .And()
            .AreNotAbstract()
            .And()
            .AreClasses()
            .Should()
            .Inherit(typeof(TreviaApp.Api.Controllers.ApiControllerBase))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(GetFailures(result));
    }

    [Fact]
    public void Controllers_Should_Not_DirectlyReference_DbContext_Or_DbSet()
    {
        var forbiddenTypes = new[]
        {
            "TreviaApp.Infrastructure.Persistence.ApplicationDbContext",
            "Microsoft.EntityFrameworkCore.DbContext",
            "Microsoft.EntityFrameworkCore.DbSet`1"
        };

        var result = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenTypes)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(GetFailures(result));
    }

    [Fact]
    public void ControllerActions_Should_Not_Return_Domain_Entities_Directly()
    {
        var domainEntityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(TreviaApp.Domain.Abstractions.Entity))
            .Or()
            .Inherit(typeof(TreviaApp.Domain.Abstractions.AggregateRoot))
            .Or()
            .Inherit(typeof(TreviaApp.Domain.Abstractions.ValueObject))
            .GetTypes()
            .Select(t => t.FullName)
            .Where(n => n != null)
            .ToArray();

        var forbidden = new List<string>(domainEntityTypes!);

        var controllers = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .And()
            .AreNotAbstract()
            .And()
            .AreClasses();

        if (forbidden.Count == 0) return;

        var result = controllers
            .ShouldNot()
            .HaveDependencyOnAny(forbidden.Take(200).ToArray())
            .GetResult();

        result.IsSuccessful.Should().BeTrue(GetFailures(result) +
            " (observação: controllers devem retornar DTOs de Contracts, não entidades de domínio)");
    }

    [Fact]
    public void Controllers_Should_Reside_In_Correct_Namespace()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespace("TreviaApp.Api.Controllers")
            .Or()
            .ResideInNamespaceStartingWith("TreviaApp.Api.Controllers.")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(GetFailures(result));
    }

    private static string GetFailures(TestResult result)
    {
        if (result.IsSuccessful) return string.Empty;
        return "Falhas: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>());
    }
}
