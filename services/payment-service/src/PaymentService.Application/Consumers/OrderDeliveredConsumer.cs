namespace PaymentService.Application.Consumers;

public class OrderDeliveredConsumer(IApplicationDbContext dbContext) : IConsumer<OrderDelivered>
{
    public async Task Consume(ConsumeContext<OrderDelivered> context)
    {
        string eventId = $"{nameof(OrderDelivered)}-{context.Message.OrderId}";

        if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        PaymentOrderLink? link = await dbContext.PaymentOrderLinks
            .Include(l => l.Payment)
            .FirstOrDefaultAsync(l => l.OrderId == context.Message.OrderId, context.CancellationToken);

        if (link is null || link.Payment!.Method != PaymentMethodType.Cod)
        {
            return;
        }

        bool alreadyHeld = await dbContext.EscrowHolds
            .AnyAsync(e => e.OrderId == context.Message.OrderId, context.CancellationToken);
        if (alreadyHeld)
        {
            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(OrderDelivered),
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (link.Payment.Status != PaymentStatus.Succeeded)
        {
            link.Payment.Status = PaymentStatus.Succeeded;
            link.Payment.UpdatedAt = now;
        }

        EscrowHold escrowHold = new()
        {
            OrderId = context.Message.OrderId,
            PaymentId = link.PaymentId,
            ShopId = link.ShopId,
            Amount = link.Amount,
            Status = EscrowStatus.Released,
            HeldAt = now,
            ReleaseDueAt = now,
            ReleasedAt = now
        };
        dbContext.EscrowHolds.Add(escrowHold);

        await ApplyWalletCreditAsync(dbContext, link.ShopId, link.Amount, escrowHold, now, context.CancellationToken);

        dbContext.ProcessedEvents.Add(new ProcessedEvent
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            EventType = nameof(OrderDelivered),
            ProcessedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }

    public static async Task ApplyWalletCreditAsync(IApplicationDbContext dbContext, Guid shopId, int amount,
        EscrowHold escrowHold, DateTimeOffset now, CancellationToken cancellationToken)
    {
        ShopWallet? wallet = await dbContext.ShopWallets
            .FirstOrDefaultAsync(w => w.ShopId == shopId, cancellationToken);

        if (wallet is null)
        {
            wallet = new ShopWallet
            {
                ShopId = shopId,
                AvailableBalance = 0,
                PendingBalance = 0,
                DebtBalance = 0,
                UpdatedAt = now
            };
            dbContext.ShopWallets.Add(wallet);
        }

        int debtSettled = Math.Min(wallet.DebtBalance, amount);
        wallet.DebtBalance -= debtSettled;
        wallet.AvailableBalance += amount - debtSettled;
        wallet.PendingBalance = Math.Max(0, wallet.PendingBalance - amount);
        wallet.UpdatedAt = now;

        dbContext.ShopWalletTransactions.Add(new ShopWalletTransaction
        {
            ShopId = shopId,
            OrderId = escrowHold.OrderId,
            EscrowHoldId = escrowHold.Id,
            Type = WalletTransactionType.EscrowRelease,
            Amount = amount,
            AvailableBalanceAfter = wallet.AvailableBalance,
            DebtBalanceAfter = wallet.DebtBalance,
            CreatedAt = now
        });

        if (debtSettled > 0)
        {
            dbContext.ShopWalletTransactions.Add(new ShopWalletTransaction
            {
                ShopId = shopId,
                OrderId = escrowHold.OrderId,
                EscrowHoldId = escrowHold.Id,
                Type = WalletTransactionType.DebtSettlement,
                Amount = -debtSettled,
                AvailableBalanceAfter = wallet.AvailableBalance,
                DebtBalanceAfter = wallet.DebtBalance,
                CreatedAt = now
            });
        }
    }
}
