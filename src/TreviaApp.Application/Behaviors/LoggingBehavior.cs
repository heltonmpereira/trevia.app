namespace TreviaApp.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Diagnostics;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Iniciando requisição {RequestName} com dados: {@Request}", requestName, Sanitize(request));
        try
        {
            var response = await next();
            stopwatch.Stop();
            _logger.LogInformation("Requisição {RequestName} concluída em {Elapsed}ms", requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro na requisição {RequestName} após {Elapsed}ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    private static string Sanitize(TRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            return json.Length > 2000 ? json.Substring(0, 2000) + "..." : json;
        }
        catch { return "[serialização falhou]"; }
    }
}
