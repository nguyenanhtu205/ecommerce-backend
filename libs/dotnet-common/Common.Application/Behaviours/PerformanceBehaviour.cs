using System.Diagnostics;

namespace Common.Application.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse>(ILogger<TRequest> logger, ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        TResponse response = await next(cancellationToken);

        _timer.Stop();

        long elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds < 500)
        {
            return response;
        }

        string requestName = typeof(TRequest).Name;
        string userId = currentUser.UserId.ToString()
                        ?? string.Empty;

        logger.LogWarning(
            "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
            requestName, elapsedMilliseconds, userId, request);

        return response;
    }
}
