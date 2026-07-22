using MediatR.Pipeline;

namespace Common.Application.Behaviours;

public class LoggingBehaviour<TRequest>(
    ILogger<TRequest> logger,
    ICurrentUser currentUser)
    : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        string userId = currentUser.UserId.ToString() ?? string.Empty;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Request: {Name} {@UserId} {@Request}",
                requestName, userId, request);
        }

        return Task.CompletedTask;
    }
}
