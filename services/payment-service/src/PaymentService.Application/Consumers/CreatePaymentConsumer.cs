namespace PaymentService.Application.Consumers;

public class CreatePaymentConsumer(
    IApplicationDbContext dbContext,
    IPaymentGatewayClient paymentGatewayClient,
    ITopicProducer<PaymentRedirectCreated> paymentRedirectCreatedProducer) : IConsumer<CreatePayment>
{
    public async Task Consume(ConsumeContext<CreatePayment> context)
    {
        if (string.Equals(context.Message.Method, "Cod", StringComparison.OrdinalIgnoreCase))
        {
            await HandleCodAsync(context);
        }
        else
        {
            await HandleVnPayAsync(context);
        }
    }

    private async Task HandleCodAsync(ConsumeContext<CreatePayment> context)
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
                PaymentId = payment.Id, OrderId = share.OrderId, Amount = share.Amount
            });
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }

    private async Task HandleVnPayAsync(ConsumeContext<CreatePayment> context)
    {
        string idempotencyKey = context.Message.CheckoutBatchId.ToString();

        Payment? existing = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, context.CancellationToken);

        if (existing is not null)
        {
            if (!string.IsNullOrEmpty(existing.RedirectUrl))
            {
                await paymentRedirectCreatedProducer.Produce(
                    new PaymentRedirectCreated(context.Message.CheckoutBatchId, existing.RedirectUrl),
                    context.CancellationToken);
            }

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
                PaymentId = payment.Id, OrderId = share.OrderId, Amount = share.Amount
            });
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
        await paymentRedirectCreatedProducer.Produce(
            new PaymentRedirectCreated(context.Message.CheckoutBatchId, redirectUrl), context.CancellationToken);
    }
}
