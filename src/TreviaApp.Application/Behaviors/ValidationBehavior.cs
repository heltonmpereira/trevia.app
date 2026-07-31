namespace TreviaApp.Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System.Text.Json;
using TreviaApp.Domain.Exceptions;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();
        ValidationContext<TRequest> context = new(request);
        ValidationResult[] results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();
        if (failures.Count != 0)
        {
            var details = JsonSerializer.Serialize(failures.ToDictionary(f => f.PropertyName, f => f.ErrorMessage));
            throw new DomainException("Um ou mais erros de validação ocorreram.", "ValidationError", details);
        }
        return await next();
    }
}
