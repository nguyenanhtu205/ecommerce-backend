namespace PaymentService.Application.Consumers;

public class CreatePaymentConsumer(
    IApplicationDbContext dbContext,
    IPaymentGatewayClient paymentGatewayClient,
    IOutboxWriter outboxWriter) : IConsumer<CreatePayment>
{
    public async Task Consume(ConsumeContext<CreatePayment> context)
    {
        string eventId = $"{nameof(CreatePayment)}-{context.Message.CheckoutBatchId}";

        if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        if (string.Equals(context.Message.Method, "cod", StringComparison.OrdinalIgnoreCase))
        {
            await HandleCodAsync(context, eventId);
        }
        else
        {
            await HandleVnPayAsync(context, eventId);
        }
    }

    private async Task HandleCodAsync(ConsumeContext<CreatePayment> context, string eventId)
    {
        foreach (OrderPaymentShare share in context.Message.OrderShares)
        {
            string idempotencyKey = share.OrderId.ToString();

            bool exists = await dbContext.Payments
                .AnyAsync(p => p.IdempotencyKey == idempotencyKey, context.CancellationToken);
            if (exists)
            {
                continue;
            }

            Payment payment = new()
            {
                BuyerId = context.Message.BuyerId,
                CheckoutBatchId = context.Message.CheckoutBatchId,
                Amount = share.Amount,
                Method = PaymentMethodType.Cod,
                Status = PaymentStatus.Pending,
                IdempotencyKey = idempotencyKey,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            dbContext.Payments.Add(payment);

            dbContext.PaymentOrderLinks.Add(new PaymentOrderLink
            {
                PaymentId = payment.Id, OrderId = share.OrderId, ShopId = share.ShopId, Amount = share.Amount
            });
        }

        dbContext.ProcessedEvents.Add(new ProcessedEvent
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            EventType = nameof(CreatePayment),
            ProcessedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }

    private async Task HandleVnPayAsync(ConsumeContext<CreatePayment> context, string eventId)
    {
        string idempotencyKey = context.Message.CheckoutBatchId.ToString();

        Payment? existing = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, context.CancellationToken);

        if (existing is not null)
        {
            if (!string.IsNullOrEmpty(existing.RedirectUrl))
            {
                outboxWriter.Enqueue(
                    new PaymentRedirectCreated(context.Message.CheckoutBatchId, existing.RedirectUrl));
            }

            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(CreatePayment),
                ProcessedAt = DateTimeOffset.UtcNow
            });

            await dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }

        string redirectUrl = await paymentGatewayClient.CreateRedirectUrlAsync(
            context.Message.CheckoutBatchId, context.Message.Amount, context.CancellationToken);

        Payment payment = new()
        {
            BuyerId = context.Message.BuyerId,
            CheckoutBatchId = context.Message.CheckoutBatchId,
            Amount = context.Message.Amount,
            Method = PaymentMethodType.VnPay,
            Status = PaymentStatus.Pending,
            IdempotencyKey = idempotencyKey,
            RedirectUrl = redirectUrl,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        dbContext.Payments.Add(payment);

        foreach (OrderPaymentShare share in context.Message.OrderShares)
        {
            dbContext.PaymentOrderLinks.Add(new PaymentOrderLink
            {
                PaymentId = payment.Id, OrderId = share.OrderId, ShopId = share.ShopId, Amount = share.Amount
            });
        }

        outboxWriter.Enqueue(new PaymentRedirectCreated(context.Message.CheckoutBatchId, redirectUrl));

        dbContext.ProcessedEvents.Add(new ProcessedEvent
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            EventType = nameof(CreatePayment),
            ProcessedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
