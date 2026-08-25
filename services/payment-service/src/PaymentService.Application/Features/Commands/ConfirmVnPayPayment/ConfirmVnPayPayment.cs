namespace PaymentService.Application.Features.Commands.ConfirmVnPayPayment;

public record ConfirmVnPayPaymentResult(string RspCode, string Message);

public record ConfirmVnPayPaymentCommand(
    IReadOnlyDictionary<string, string> QueryParameters) : IRequest<ConfirmVnPayPaymentResult>;

public class ConfirmVnPayPaymentCommandHandler(
    IVnPaySignatureVerifier signatureVerifier,
    IApplicationDbContext dbContext,
    IOutboxWriter outboxWriter)
    : IRequestHandler<ConfirmVnPayPaymentCommand, ConfirmVnPayPaymentResult>
{
    public async Task<ConfirmVnPayPaymentResult> Handle(
        ConfirmVnPayPaymentCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<string, string> query = command.QueryParameters;

        if (!query.TryGetValue("vnp_SecureHash", out string? receivedHash) ||
            string.IsNullOrEmpty(receivedHash) ||
            !signatureVerifier.Verify(query, receivedHash))
        {
            return new ConfirmVnPayPaymentResult("97", "Invalid signature");
        }

        if (!query.TryGetValue("vnp_TxnRef", out string? txnRef)
            || !Guid.TryParseExact(txnRef, "N", out Guid checkoutBatchId))
        {
            return new ConfirmVnPayPaymentResult("01", "Invalid TxnRef");
        }

        string idempotencyKey = checkoutBatchId.ToString();
        Payment? payment = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, cancellationToken);

        if (payment is null)
        {
            return new ConfirmVnPayPaymentResult("01", "Order not found");
        }

        if (!query.TryGetValue("vnp_Amount", out string? vnpAmount)
            || !long.TryParse(vnpAmount, out long amountX100)
            || amountX100 != payment.Amount * 100L)
        {
            return new ConfirmVnPayPaymentResult("04", "Invalid amount");
        }

        if (payment.Status is PaymentStatus.Succeeded or PaymentStatus.Failed)
        {
            return new ConfirmVnPayPaymentResult("02", "Order already confirmed");
        }

        string? responseCode = query.GetValueOrDefault("vnp_ResponseCode");
        bool isSuccess = responseCode == "00";
        string? providerTransactionId = query.GetValueOrDefault("vnp_TransactionNo");

        payment.Status = isSuccess ? PaymentStatus.Succeeded : PaymentStatus.Failed;
        payment.ProviderTransactionId = providerTransactionId;
        payment.UpdatedAt = DateTimeOffset.UtcNow;

        if (isSuccess)
        {
            await CreateEscrowHoldsAsync(checkoutBatchId, cancellationToken);
        }

        outboxWriter.Enqueue(new VnPayPaymentConfirmed(
            checkoutBatchId, isSuccess, providerTransactionId,
            isSuccess ? null : $"VNPay response code: {responseCode}"));

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ConfirmVnPayPaymentResult("00", "Confirm Success");
    }

    private async Task CreateEscrowHoldsAsync(Guid checkoutBatchId, CancellationToken cancellationToken)
    {
        List<PaymentOrderLink> links = await dbContext.PaymentOrderLinks
            .Include(l => l.Payment)
            .Where(l => l.Payment!.CheckoutBatchId == checkoutBatchId)
            .ToListAsync(cancellationToken);

        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (PaymentOrderLink link in links)
        {
            bool alreadyHeld = await dbContext.EscrowHolds
                .AnyAsync(e => e.OrderId == link.OrderId, cancellationToken);
            if (alreadyHeld)
            {
                continue;
            }

            dbContext.EscrowHolds.Add(new EscrowHold
            {
                OrderId = link.OrderId,
                PaymentId = link.PaymentId,
                ShopId = link.ShopId,
                Amount = link.Amount,
                Status = EscrowStatus.Held,
                HeldAt = now,
                ReleaseDueAt = now.AddDays(7)
            });
        }
    }
}
