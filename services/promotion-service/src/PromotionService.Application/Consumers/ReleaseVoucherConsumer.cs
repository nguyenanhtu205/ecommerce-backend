using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PromotionService.Application.Consumers;

public class ReleaseVoucherConsumer(IApplicationDbContext dbContext) : IConsumer<ReleaseVoucher>
{
    private const int MaxConcurrencyRetries = 3;

    public async Task Consume(ConsumeContext<ReleaseVoucher> context)
    {
        string eventId = $"{nameof(ReleaseVoucher)}-{string.Join(",", context.Message.OrderIds.OrderBy(id => id))}";

        if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        for (int attempt = 0; attempt < MaxConcurrencyRetries; attempt++)
        {
            List<VoucherRedemption> redemptions = await dbContext.VoucherRedemptions
                .Where(r => context.Message.OrderIds.Contains(r.OrderId))
                .ToListAsync(context.CancellationToken);

            foreach (VoucherRedemption redemption in redemptions)
            {
                Voucher? voucher = await dbContext.Vouchers
                    .FirstOrDefaultAsync(v => v.Id == redemption.VoucherId, context.CancellationToken);

                voucher?.QuantityUsed = Math.Max(0, voucher.QuantityUsed - 1);

                dbContext.VoucherRedemptions.Remove(redemption);
            }

            bool alreadyProcessed = await dbContext.ProcessedEvents
                .AnyAsync(e => e.EventId == eventId, context.CancellationToken);
            if (!alreadyProcessed)
            {
                dbContext.ProcessedEvents.Add(new ProcessedEvent
                {
                    Id = Guid.CreateVersion7(),
                    EventId = eventId,
                    EventType = nameof(ReleaseVoucher),
                    ProcessedAt = DateTimeOffset.UtcNow
                });
            }

            try
            {
                await dbContext.SaveChangesAsync(context.CancellationToken);
                return;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (EntityEntry entry in ex.Entries)
                {
                    entry.State = EntityState.Detached;
                }

                if (attempt == MaxConcurrencyRetries - 1)
                {
                    throw;
                }
            }
        }
    }
}
