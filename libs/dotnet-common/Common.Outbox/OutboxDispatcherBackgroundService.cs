using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.Outbox;

public class OutboxDispatcherBackgroundService<TDbContext>(
    IServiceScopeFactory scopeFactory,
    IHostApplicationLifetime lifetime) : BackgroundService
    where TDbContext : class, IOutboxDbContext
{
    private const int BatchSize = 50;
    private const int MaxAttempts = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TaskCompletionSource startedTcs = new();
        await using CancellationTokenRegistration reg =
            lifetime.ApplicationStarted.Register(() => startedTcs.TrySetResult());
        await startedTcs.Task.WaitAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            Dictionary<string, IOutboxMessageDispatcher> dispatchers = scope.ServiceProvider
                .GetServices<IOutboxMessageDispatcher>()
                .ToDictionary(d => d.MessageType);

            List<OutboxMessage> pending = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt == null && m.AttemptCount < MaxAttempts)
                .OrderBy(m => m.CreatedAt)
                .Take(BatchSize)
                .ToListAsync(stoppingToken);

            foreach (OutboxMessage message in pending)
            {
                if (!dispatchers.TryGetValue(message.MessageType, out IOutboxMessageDispatcher? dispatcher))
                {
                    message.Error = $"No dispatcher registered for {message.MessageType}";
                    message.AttemptCount++;
                    continue;
                }

                try
                {
                    await dispatcher.DispatchAsync(message.Payload, stoppingToken);
                    message.ProcessedAt = DateTimeOffset.UtcNow;
                }
                catch (Exception ex)
                {
                    message.AttemptCount++;
                    message.Error = ex.Message;
                }
            }

            if (pending.Count > 0)
            {
                await dbContext.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
