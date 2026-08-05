namespace PaymentService.Domain.Enums;

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    Refunded,
    PartiallyRefunded
}
