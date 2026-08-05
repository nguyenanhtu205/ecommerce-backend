namespace OrderService.Application.Sagas.Checkout;

public class CheckoutSagaState : SagaStateMachineInstance
{
    public required string CurrentState { get; set; }

    public Guid BuyerId { get; set; }

    public List<Guid> OrderIds { get; set; } = [];

    public List<Guid> ReservedOrderIds { get; set; } = [];

    public int TotalAmount { get; set; }

    public List<OrderPaymentShare> OrderShares { get; set; } = [];

    public required string PaymentMethod { get; set; }

    public string? PlatformVoucherCode { get; set; }

    public List<ShopVoucherRedemption> ShopVouchers { get; set; } = [];

    public bool VoucherRedeemed { get; set; }

    public string? RedirectUrl { get; set; }

    public string? FailReason { get; set; }

    public Guid CorrelationId { get; set; }
}
