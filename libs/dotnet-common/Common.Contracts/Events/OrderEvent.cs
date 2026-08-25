namespace Common.Contracts.Events;

public record CheckoutInitiated(
    Guid CheckoutBatchId,
    Guid BuyerId,
    List<Guid> OrderIds,
    string PaymentMethod,
    int TotalAmount,
    List<OrderPaymentShare> OrderShares,
    string? PlatformVoucherCode,
    List<ShopVoucherRedemption> ShopVouchers);

public record OrderPaymentShare(Guid OrderId, Guid ShopId, int Amount);

public record ShopVoucherRedemption(Guid ShopId, Guid OrderId, string VoucherCode, int DiscountAmount);

public record RedeemVoucher(
    Guid CheckoutBatchId,
    Guid BuyerId,
    string? PlatformVoucherCode,
    List<ShopVoucherRedemption> ShopVouchers,
    List<OrderPaymentShare> OrderShares);

public record CreatePayment(
    Guid CheckoutBatchId,
    Guid BuyerId,
    int Amount,
    string Method,
    List<OrderPaymentShare> OrderShares);

public record ReleaseVoucher(Guid CheckoutBatchId, List<Guid> OrderIds);

public record OrderPaymentSucceeded(Guid CheckoutBatchId, Guid OrderId);

public record OrderPaymentFailed(Guid CheckoutBatchId, Guid OrderId, string Reason);

public record ReleaseStockCommand(Guid OrderId);

public record CancelOrder(Guid OrderId, string Reason, string InitiatedBy);

public record OrderReadyItem(Guid CombinationId, int Quantity);

public record CheckoutAddressSnapshot(
    Guid UserId,
    string FullName,
    string Phone,
    string Province,
    string Ward,
    string AddressDetail,
    string FullAddressText,
    decimal? Latitude,
    decimal? Longitude,
    string AddressType
);

public record ReserveOrderStock(
    Guid CheckoutBatchId,
    Guid OrderId,
    List<OrderReadyItem> Items,
    Guid CarrierId,
    CheckoutAddressSnapshot PickupAddressSnapshot,
    CheckoutAddressSnapshot DeliveryAddressSnapshot);

public record ReserveStockItem(Guid CombinationId, int Quantity);

public record ReserveStock(Guid OrderId, List<ReserveStockItem> Items);

public record OrderStockReserved(Guid CheckoutBatchId, Guid OrderId);

public record OrderStockReservationFailed(Guid CheckoutBatchId, Guid OrderId, string Reason);

public record CreateShipment(
    Guid OrderId,
    CheckoutAddressSnapshot PickupAddressSnapshot,
    CheckoutAddressSnapshot DeliveryAddressSnapshot,
    Guid CarrierId);

public record CommitStockCommand(Guid OrderId);

public record OrderCompletedItem(Guid OrderItemId, Guid ProductId, Guid CombinationId, string? Variation, int Quantity);

public record OrderCompleted(
    Guid OrderId,
    Guid ShopId,
    Guid BuyerId,
    DateTimeOffset CompletedAt,
    List<OrderCompletedItem> Items);
