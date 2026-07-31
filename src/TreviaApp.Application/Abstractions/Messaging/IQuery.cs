using MediatR;

namespace TreviaApp.Application.Abstractions.Messaging;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
