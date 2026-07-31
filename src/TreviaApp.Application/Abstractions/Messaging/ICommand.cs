namespace TreviaApp.Application.Abstractions.Messaging;
using MediatR;

public interface ICommand : IRequest, IBaseCommand;
public interface ICommand<TResponse> : IRequest<TResponse>, IBaseCommand;
public interface IBaseCommand;
