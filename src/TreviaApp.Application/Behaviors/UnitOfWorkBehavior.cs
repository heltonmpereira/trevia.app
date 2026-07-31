namespace TreviaApp.Application.Behaviors;
using MediatR;
using TreviaApp.Domain.Interfaces;

public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    public UnitOfWorkBehavior(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();
        if (typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal) || typeof(TRequest).GetInterfaces().Any(i => i.Name.StartsWith("ICommand")))
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }
}
