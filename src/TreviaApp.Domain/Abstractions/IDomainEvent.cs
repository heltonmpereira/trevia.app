using MediatR;

namespace TreviaApp.Domain.Abstractions;

/// <summary>
/// Defines the IDomainEvent contract.
/// </summary>
public interface IDomainEvent : INotification
{
}
