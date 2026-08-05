namespace PaymentService.Application.Consumers;

public class OrderDeliveredConsumer(IApplicationDbContext dbContext) : IConsumer<OrderDelivered>
{
    public async Task Consume(ConsumeContext<OrderDelivered> context)
    {
        PaymentOrderLink? link = await dbContext.PaymentOrderLinks
            .Include(l => l.Payment)
            .FirstOrDefaultAsync(l => l.OrderId == context.Message.OrderId, context.CancellationToken);

        if (link is null || link.Payment!.Method != PaymentMethodType.Cod)
        {
            return;
        }

        if (link.Payment.Status == PaymentStatus.Succeeded)
        {
            return;
        }

        link.Payment.Status = PaymentStatus.Succeeded;
        link.Payment.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
