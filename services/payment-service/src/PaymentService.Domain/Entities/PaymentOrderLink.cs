namespace PaymentService.Domain.Entities;

public class PaymentOrderLink : BaseEntity
{
    public required Guid PaymentId { get; init; }

    public required Guid OrderId { get; init; }

    public required int Amount { get; init; }

    public Payment? Payment { get; init; }
}
