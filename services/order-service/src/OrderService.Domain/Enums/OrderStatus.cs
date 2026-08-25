namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    PendingPayment,
    Shipping,
    Completed,
    Cancelled,
    ReturnRefund
}
