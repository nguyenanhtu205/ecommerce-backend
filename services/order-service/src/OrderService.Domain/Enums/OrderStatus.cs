namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    PendingPayment,
    PendingConfirmation,
    Shipping,
    PendingDelivery,
    Completed,
    Cancelled,
    ReturnRefund
}
