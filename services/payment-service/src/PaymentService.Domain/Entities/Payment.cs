namespace PaymentService.Domain.Entities;

public class Payment : BaseEntity
{
    public required Guid BuyerId { get; init; }

    public required Guid CheckoutBatchId { get; init; }

    public required int Amount { get; init; }

    public required PaymentMethodType Method { get; init; }

    public required PaymentStatus Status { get; set; }

    public required string IdempotencyKey { get; init; }

    public string? ProviderTransactionId { get; set; }

    public string? RedirectUrl { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Refund> Refunds { get; private set; } = new List<Refund>();

    public ICollection<PaymentOrderLink> PaymentOrderLinks { get; private set; } = new List<PaymentOrderLink>();

    public ICollection<EscrowHold> EscrowHolds { get; private set; } = new List<EscrowHold>();
}
