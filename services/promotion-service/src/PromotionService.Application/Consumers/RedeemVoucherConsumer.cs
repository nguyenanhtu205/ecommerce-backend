namespace PromotionService.Application.Consumers;

public class RedeemVoucherConsumer(IApplicationDbContext dbContext, IOutboxWriter outboxWriter)
    : IConsumer<RedeemVoucher>
{
    private const int MaxConcurrencyRetries = 3;

    public async Task Consume(ConsumeContext<RedeemVoucher> context)
    {
        string eventId = $"{nameof(RedeemVoucher)}-{context.Message.CheckoutBatchId}";

        if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        for (int attempt = 0; attempt < MaxConcurrencyRetries; attempt++)
        {
            string? failureReason = null;

            foreach (ShopVoucherRedemption shopVoucher in context.Message.ShopVouchers)
            {
                Voucher? voucher = await dbContext.Vouchers
                    .FirstOrDefaultAsync(v => v.Code == shopVoucher.VoucherCode, context.CancellationToken);

                if (voucher is null)
                {
                    failureReason = $"Voucher {shopVoucher.VoucherCode} does not exist.";
                    break;
                }

                bool alreadyRedeemed = await dbContext.VoucherRedemptions
                    .AnyAsync(r => r.VoucherId == voucher.Id && r.OrderId == shopVoucher.OrderId,
                        context.CancellationToken);
                if (alreadyRedeemed)
                {
                    continue;
                }

                if (voucher.QuantityLimit.HasValue && voucher.QuantityUsed >= voucher.QuantityLimit.Value)
                {
                    failureReason = $"Voucher {shopVoucher.VoucherCode} has reached its usage limit.";
                    break;
                }

                dbContext.VoucherRedemptions.Add(new VoucherRedemption
                {
                    VoucherId = voucher.Id,
                    OrderId = shopVoucher.OrderId,
                    UserId = context.Message.BuyerId,
                    DiscountAmount = shopVoucher.DiscountAmount,
                    RedeemedAt = DateTimeOffset.UtcNow
                });
                voucher.QuantityUsed += 1;
            }

            if (failureReason is not null)
            {
                dbContext.ChangeTracker.Clear();

                outboxWriter.Enqueue(new VoucherRedemptionFailed(context.Message.CheckoutBatchId, failureReason));

                dbContext.ProcessedEvents.Add(new ProcessedEvent
                {
                    Id = Guid.CreateVersion7(),
                    EventId = eventId,
                    EventType = nameof(RedeemVoucher),
                    ProcessedAt = DateTimeOffset.UtcNow
                });

                await dbContext.SaveChangesAsync(context.CancellationToken);
                return;
            }

            outboxWriter.Enqueue(new VoucherRedeemed(context.Message.CheckoutBatchId));

            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(RedeemVoucher),
                ProcessedAt = DateTimeOffset.UtcNow
            });

            try
            {
                await dbContext.SaveChangesAsync(context.CancellationToken);
                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                dbContext.ChangeTracker.Clear();

                if (attempt == MaxConcurrencyRetries - 1)
                {
                    throw;
                }
            }
        }
    }
}
