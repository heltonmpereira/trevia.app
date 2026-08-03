namespace TreviaApp.ArchitectureTests;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

public class NamingConventionTests
{
    private static readonly System.Reflection.Assembly ApplicationAssembly =
        typeof(TreviaApp.Application.DependencyInjection.ServiceCollectionExtensions).Assembly;

    private static readonly System.Reflection.Assembly DomainAssembly =
        typeof(TreviaApp.Domain.Abstractions.Entity).Assembly;

    [Fact]
    public void CommandHandlers_Should_HaveCorrectSuffix()
    {
        var handlers = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(TreviaApp.Application.Abstractions.Messaging.ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(TreviaApp.Application.Abstractions.Messaging.ICommandHandler<>))
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        handlers.IsSuccessful.Should().BeTrue(GetFailures(handlers, "ICommandHandler implementations must end with 'CommandHandler'"));
    }

    [Fact]
    public void QueryHandlers_Should_HaveCorrectSuffix()
    {
        var handlers = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(TreviaApp.Application.Abstractions.Messaging.IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        handlers.IsSuccessful.Should().BeTrue(GetFailures(handlers, "IQueryHandler implementations must end with 'QueryHandler'"));
    }

    [Fact]
    public void Validators_Should_Reside_In_Application_And_Have_Suffix()
    {
        var validators = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        validators.IsSuccessful.Should().BeTrue(GetFailures(validators,
            "FluentValidation AbstractValidator subclasses must end with 'Validator'"));
    }

    [Fact]
    public void Domain_Entities_Should_Not_Have_Service_Or_Manager_Suffix()
    {
        var forbiddenSuffixes = new[] { "Service", "Manager", "Provider", "Factory", "Repository", "Handler" };

        foreach (var suffix in forbiddenSuffixes)
        {
            var bad = Types.InAssembly(DomainAssembly)
                .That()
                .Inherit(typeof(TreviaApp.Domain.Abstractions.Entity))
                .Or()
                .Inherit(typeof(TreviaApp.Domain.Abstractions.AggregateRoot))
                .Or()
                .Inherit(typeof(TreviaApp.Domain.Abstractions.ValueObject))
                .ShouldNot()
                .HaveNameEndingWith(suffix)
                .GetResult();

            bad.IsSuccessful.Should().BeTrue(GetFailures(bad,
                $"Domain entities should not end with '{suffix}'"));
        }
    }

    [Fact]
    public void Commands_Should_Have_Suffix_Command()
    {
        var commands = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(TreviaApp.Application.Abstractions.Messaging.ICommand))
            .Or()
            .ImplementInterface(typeof(TreviaApp.Application.Abstractions.Messaging.ICommand<>))
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        commands.IsSuccessful.Should().BeTrue(GetFailures(commands, "ICommand types must end with 'Command'"));
    }

    [Fact]
    public void Queries_Should_Have_Suffix_Query()
    {
        var queries = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(TreviaApp.Application.Abstractions.Messaging.IQuery<>))
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        queries.IsSuccessful.Should().BeTrue(GetFailures(queries, "IQuery types must end with 'Query'"));
    }

    private static string GetFailures(TestResult result, string? extra = null)
    {
        if (result.IsSuccessful) return string.Empty;
        var failures = "Falhas: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>());
        if (extra != null) failures += " — " + extra;
        return failures;
    }
}
