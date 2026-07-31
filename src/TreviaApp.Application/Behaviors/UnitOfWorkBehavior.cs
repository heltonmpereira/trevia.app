namespace TreviaApp.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using TreviaApp.Domain.Interfaces;

public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnitOfWorkBehavior<TRequest, TResponse>> _logger;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork, ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is TreviaApp.Application.Abstractions.Messaging.IBaseCommand)
        {
            var affected = await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("UnitOfWorkBehavior: SaveChangesAsync executado — {Rows} linhas afetadas para {RequestType}", affected, typeof(TRequest).Name);
        }
        else
        {
            _logger.LogDebug("UnitOfWorkBehavior: {RequestType} não é IBaseCommand — pulando SaveChanges (é Query).", typeof(TRequest).Name);
        }

        return response;
    }
}
