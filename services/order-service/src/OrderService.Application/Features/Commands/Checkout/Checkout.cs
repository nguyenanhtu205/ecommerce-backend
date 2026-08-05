namespace OrderService.Application.Features.Commands.Checkout;

public record CheckoutItem(
    Guid ShopId,
    Guid CombinationId,
    int Quantity,
    Guid ProductId,
    string ProductName,
    string ThumbnailUrl,
    string? Variation);

public record ShopCheckoutInfo(Guid ShopId, Guid CarrierId, string? ShopVoucherCode, string? Note);

public record CheckoutResult(
    bool Success,
    Guid CheckoutBatchId,
    List<Guid> OrderIds,
    string? RedirectUrl,
    string? FailureReason);

public record CheckoutCommand(
    List<CheckoutItem> CartItems,
    List<ShopCheckoutInfo> ShopInfos,
    AddressSnapshot ShippingAddressSnapshot,
    string PaymentMethod,
    string? PlatformVoucherCode) : IRequest<CheckoutResult>;

public class CheckoutCommandHandler(
    IInventoryServiceClient inventoryClient,
    IPromotionServiceClient promotionClient,
    IShippingServiceClient shippingClient,
    IShopServiceClient shopClient,
    IApplicationDbContext context,
    ITopicProducer<CheckoutInitiated> checkoutInitiatedProducer,
    ITopicProducer<ReserveOrderStock> reserveOrderStockProducer,
    ICurrentUser currentUser) : IRequestHandler<CheckoutCommand, CheckoutResult>
{
    public async Task<CheckoutResult> Handle(CheckoutCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid buyerId = currentUser.UserId.Value;

        if (command.CartItems.Count == 0 || command.ShopInfos.Count == 0)
        {
            return new CheckoutResult(false, Guid.Empty, [], null, "Empty cart");
        }

        Dictionary<Guid, List<CheckoutItem>> itemsByShop =
            command.CartItems.GroupBy(i => i.ShopId).ToDictionary(g => g.Key, g => g.ToList());
        Dictionary<Guid, ShopCheckoutInfo> infoByShop = command.ShopInfos.ToDictionary(s => s.ShopId);

        if (itemsByShop.Keys.Any(shopId => !infoByShop.ContainsKey(shopId)))
        {
            return new CheckoutResult(false, Guid.Empty, [], null,
                "Missing carrier/voucher information for one shop in the cart.");
        }

        IEnumerable<Guid> combinationIds = command.CartItems.Select(i => i.CombinationId).Distinct();
        IReadOnlyDictionary<Guid, CombinationPriceInfo> priceMap =
            await inventoryClient.GetPricesAsync(combinationIds, cancellationToken);
        if (command.CartItems.Any(i => !priceMap.ContainsKey(i.CombinationId)))
        {
            return new CheckoutResult(false, Guid.Empty, [], null,
                "Some products no longer exist or are no longer available for sale.");
        }

        Guid checkoutBatchId = Guid.NewGuid();
        List<Guid> orderIds = [];
        List<OrderPaymentShare> orderShares = [];
        List<ShopVoucherRedemption> shopVouchers = [];
        List<ReserveOrderStock> reserveCommandsPerOrder = [];

        Dictionary<Guid, int> shopSubtotals = new();
        Dictionary<Guid, ShippingFeeResult> shopShippingFees = new();
        Dictionary<Guid, int> shopDiscounts = new();
        Dictionary<Guid, AddressSnapshot> shopPickupAddresses = new();

        foreach ((Guid shopId, List<CheckoutItem> items) in itemsByShop)
        {
            ShopCheckoutInfo info = infoByShop[shopId];
            int subtotal = items.Sum(i => priceMap[i.CombinationId].Price * i.Quantity);
            shopSubtotals[shopId] = subtotal;

            ShopPickupAddressResult pickupAddressResult =
                await shopClient.GetPickupAddressAsync(shopId, cancellationToken);
            if (!pickupAddressResult.IsValid || pickupAddressResult.PickupAddressSnapshot is null)
            {
                return new CheckoutResult(false, Guid.Empty, [], null,
                    $"Failed to retrieve pickup address for shop {shopId}: {pickupAddressResult.FailureReason}");
            }

            shopPickupAddresses[shopId] = pickupAddressResult.PickupAddressSnapshot;

            ShippingFeeResult feeResult = await shippingClient.CalculateFeeAsync(
                new ShippingFeeRequest(info.CarrierId, shopPickupAddresses[shopId], command.ShippingAddressSnapshot),
                cancellationToken);
            if (!feeResult.IsValid)
            {
                return new CheckoutResult(false, Guid.Empty, [], null,
                    $"Unable to calculate the shipping fee for shop {shopId}: {feeResult.FailureReason}");
            }

            shopShippingFees[shopId] = feeResult;

            int shopDiscount = 0;
            if (!string.IsNullOrWhiteSpace(info.ShopVoucherCode))
            {
                VoucherDryRunResult dryRun = await promotionClient.DryRunCalculateDiscountAsync(
                    new VoucherDryRunRequest(info.ShopVoucherCode, shopId, buyerId, subtotal),
                    cancellationToken);
                if (!dryRun.IsValid)
                {
                    return new CheckoutResult(false, Guid.Empty, [], null,
                        $"Voucher shop {info.ShopVoucherCode} invalid: {dryRun.FailureReason}");
                }

                shopDiscount = dryRun.DiscountAmount;
            }

            shopDiscounts[shopId] = shopDiscount;
        }

        int cartMerchandiseTotal = shopSubtotals.Values.Sum();
        int platformDiscount = 0;
        if (!string.IsNullOrWhiteSpace(command.PlatformVoucherCode))
        {
            VoucherDryRunResult dryRun = await promotionClient.DryRunCalculateDiscountAsync(
                new VoucherDryRunRequest(command.PlatformVoucherCode, null, buyerId, cartMerchandiseTotal),
                cancellationToken);
            if (!dryRun.IsValid)
            {
                return new CheckoutResult(false, Guid.Empty, [], null,
                    $"Voucher platform không hợp lệ: {dryRun.FailureReason}");
            }

            platformDiscount = dryRun.DiscountAmount;
        }

        foreach ((Guid shopId, List<CheckoutItem> items) in itemsByShop)
        {
            ShopCheckoutInfo info = infoByShop[shopId];
            int subtotal = shopSubtotals[shopId];
            int shippingFee = shopShippingFees[shopId].Fee;
            int shopDiscount = shopDiscounts[shopId];

            int platformShare = cartMerchandiseTotal == 0
                ? 0
                : (int)Math.Round(platformDiscount * (decimal)subtotal / cartMerchandiseTotal);

            int totalDiscount = shopDiscount + platformShare;
            int totalPayment = subtotal + shippingFee - totalDiscount;

            Order order = new()
            {
                BuyerId = buyerId,
                ShopId = shopId,
                CheckoutBatchId = checkoutBatchId,
                Status = OrderStatus.PendingPayment,
                MerchandiseSubtotal = subtotal,
                ShippingFee = shippingFee,
                VoucherDiscount = totalDiscount,
                XuDiscount = 0,
                TotalPayment = totalPayment,
                ShippingAddressSnapshot = command.ShippingAddressSnapshot,
                Note = info.Note,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Orders.Add(order);

            orderIds.Add(order.Id);

            foreach (CheckoutItem item in items)
            {
                CombinationPriceInfo priceInfo = priceMap[item.CombinationId];
                context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    CombinationId = item.CombinationId,
                    ProductName = item.ProductName,
                    ThumbnailUrl = item.ThumbnailUrl,
                    Variation = item.Variation,
                    Quantity = item.Quantity,
                    Price = priceInfo.Price,
                    OriginalPrice = null
                });
            }

            context.OrderShippingSnapshots.Add(new OrderShippingSnapshot
            {
                OrderId = order.Id,
                CarrierId = info.CarrierId,
                CarrierName = shopShippingFees[shopId].CarrierName ?? string.Empty,
                Fee = shippingFee,
                EstimatedDeliveryStart = shopShippingFees[shopId].EstimatedStart,
                EstimatedDeliveryEnd = shopShippingFees[shopId].EstimatedEnd
            });

            if (shopDiscount > 0 && !string.IsNullOrWhiteSpace(info.ShopVoucherCode))
            {
                context.OrderVouchers.Add(new OrderVoucher
                {
                    OrderId = order.Id,
                    VoucherCode = info.ShopVoucherCode,
                    DiscountAmount = shopDiscount,
                    Scope = "shop"
                });
                shopVouchers.Add(new ShopVoucherRedemption(shopId, order.Id, info.ShopVoucherCode!, shopDiscount));
            }

            if (platformShare > 0 && command.PlatformVoucherCode is not null)
            {
                context.OrderVouchers.Add(new OrderVoucher
                {
                    OrderId = order.Id,
                    VoucherCode = command.PlatformVoucherCode,
                    DiscountAmount = platformShare,
                    Scope = "platform"
                });
            }

            context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.PendingPayment,
                ChangedAt = DateTimeOffset.UtcNow,
                ChangedBy = "buyer"
            });

            orderShares.Add(new OrderPaymentShare(order.Id, totalPayment));

            reserveCommandsPerOrder.Add(new ReserveOrderStock(
                checkoutBatchId, order.Id,
                [.. items.Select(i => new OrderReadyItem(i.CombinationId, i.Quantity))],
                info.CarrierId,
                AddressMapper.ToCheckoutAddressSnapshot(shopPickupAddresses[shopId]),
                AddressMapper.ToCheckoutAddressSnapshot(command.ShippingAddressSnapshot)
            ));
        }

        int totalAmount = orderShares.Sum(s => s.Amount);

        await checkoutInitiatedProducer.Produce(new CheckoutInitiated(
            checkoutBatchId, buyerId, orderIds, command.PaymentMethod, totalAmount,
            orderShares, command.PlatformVoucherCode, shopVouchers), cancellationToken);

        foreach (ReserveOrderStock reserveCommand in reserveCommandsPerOrder)
        {
            await reserveOrderStockProducer.Produce(reserveCommand, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CheckoutResult(true, checkoutBatchId, orderIds, null, null);
    }
}
