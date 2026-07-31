using MediatR;

namespace TreviaApp.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
