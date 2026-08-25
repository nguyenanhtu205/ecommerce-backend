using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentService.Application.Consumers;

namespace PaymentService.Infrastructure.Services;

public class EscrowAutoReleaseJob(IServiceScopeFactory scopeFactory, TimeProvider timeProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            IApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            DateTimeOffset now = timeProvider.GetUtcNow();
            List<EscrowHold> dueHolds = await dbContext.EscrowHolds
                .Where(e => e.Status == EscrowStatus.Held && e.ReleaseDueAt < now)
                .ToListAsync(stoppingToken);

            foreach (EscrowHold hold in dueHolds)
            {
                hold.Status = EscrowStatus.Released;
                hold.ReleasedAt = now;

                await OrderDeliveredConsumer.ApplyWalletCreditAsync(
                    dbContext, hold.ShopId, hold.Amount, hold, now, stoppingToken);
            }

            if (dueHolds.Count > 0)
            {
                await dbContext.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
