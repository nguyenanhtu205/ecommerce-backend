namespace OrderService.Application.Features.Commands.Checkout;

public record CheckoutResult(
    bool Success,
    Guid CheckoutBatchId,
    List<Guid> OrderIds,
    int? TotalAmount,
    string? RedirectUrl,
    string? FailureReason);

public record CheckoutItem(Guid CombinationId, int Quantity, string Variation);

public record ShopCheckoutInfo(Guid ShopId, string CarrierCode, string? ShopVoucherCode, string? Note);

public record CheckoutCommand(
    List<CheckoutItem> CartItems,
    List<ShopCheckoutInfo> ShopInfos,
    Guid ShippingAddressId,
    string PaymentMethod,
    string? PlatformVoucherCode) : IRequest<CheckoutResult>;

public class Checkout(
    IInventoryServiceClient inventoryClient,
    IProductServiceClient productServiceClient,
    IUserServiceClient userServiceClient,
    IPromotionServiceClient promotionClient,
    IShippingServiceClient shippingClient,
    IShopServiceClient shopClient,
    IApplicationDbContext context,
    IOutboxWriter outboxWriter,
    ICurrentUser currentUser) : IRequestHandler<CheckoutCommand, CheckoutResult>
{
    private static readonly Dictionary<string, (Guid Id, string Name)> Carriers = new()
    {
        ["mock"] = (Guid.Parse("11111111-1111-1111-1111-111111111111"), "Giao Hàng Thử Nghiệm"),
        ["ghn"] = (Guid.Parse("22222222-2222-2222-2222-222222222222"), "Giao Hàng Nhanh"),
        ["ghtk"] = (Guid.Parse("33333333-3333-3333-3333-333333333333"), "Giao Hàng Tiết Kiệm")
    };

    public async Task<CheckoutResult> Handle(CheckoutCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid buyerId = currentUser.UserId.Value;

        if (command.CartItems.Count == 0 || command.ShopInfos.Count == 0)
        {
            return new CheckoutResult(false, Guid.Empty, [], null, null, "Empty cart");
        }

        Dictionary<Guid, ShopCheckoutInfo> infoByShop = command.ShopInfos.ToDictionary(s => s.ShopId);

        IEnumerable<Guid> combinationIds = command.CartItems.Select(i => i.CombinationId).Distinct();
        IReadOnlyDictionary<Guid, CombinationPriceInfo> priceMap =
            await inventoryClient.GetPricesAsync(combinationIds, cancellationToken);
        if (command.CartItems.Any(i => !priceMap.ContainsKey(i.CombinationId)))
        {
            return new CheckoutResult(false, Guid.Empty, [], null, null,
                "Some products no longer exist or are no longer available for sale.");
        }

        Dictionary<Guid, List<CheckoutItem>> itemsByShop = command.CartItems
            .GroupBy(i => priceMap[i.CombinationId].ShopId)
            .ToDictionary(g => g.Key, g => g.ToList());

        if (itemsByShop.Keys.Any(shopId => !infoByShop.ContainsKey(shopId)))
        {
            return new CheckoutResult(false, Guid.Empty, [], null, null,
                "Missing carrier/voucher information for one shop in the cart, or ShopId mismatch.");
        }

        List<Guid> productIds =
        [
            .. command.CartItems
                .Select(i => priceMap[i.CombinationId].ProductId).Distinct()
        ];
        IReadOnlyDictionary<Guid, ProductInfo> productInfoMap =
            await productServiceClient.GetProductInfosAsync(productIds, cancellationToken);
        if (productIds.Any(id => !productInfoMap.ContainsKey(id)))
        {
            return new CheckoutResult(false, Guid.Empty, [], null, null,
                "Some products no longer exist or are no longer available for sale.");
        }

        IReadOnlyCollection<UserShippingAddress> userAddresses =
            await userServiceClient.GetUserShippingAddressesAsync(buyerId, cancellationToken);
        UserShippingAddress? matchedAddress = userAddresses
            .FirstOrDefault(a => a.Id == command.ShippingAddressId);
        if (matchedAddress is null)
        {
            return new CheckoutResult(false, Guid.Empty, [], null, null,
                "Shipping address does not belong to this user or does not exist.");
        }

        AddressSnapshot deliveryAddress = matchedAddress.ShippingAddressSnapshot;

        Guid checkoutBatchId = Guid.NewGuid();
        List<Guid> orderIds = [];
        List<OrderPaymentShare> orderShares = [];
        List<ShopVoucherRedemption> shopVouchers = [];
        List<ReserveOrderStock> reserveCommandsPerOrder = [];

        Dictionary<Guid, int> shopSubtotals = new();
        Dictionary<Guid, ShippingFeeResult> shopShippingFees = new();
        Dictionary<Guid, int> shopDiscounts = new();
        Dictionary<Guid, AddressSnapshot> shopPickupAddresses = new();
        Dictionary<Guid, string> shopNames = new();

        foreach ((Guid shopId, List<CheckoutItem> items) in itemsByShop)
        {
            ShopCheckoutInfo info = infoByShop[shopId];
            int subtotal = items.Sum(i => priceMap[i.CombinationId].Price * i.Quantity);
            shopSubtotals[shopId] = subtotal;

            ShopPickupAddressResult pickupAddressResult =
                await shopClient.GetPickupAddressAsync(shopId, cancellationToken);
            if (!pickupAddressResult.IsValid || pickupAddressResult.PickupAddressSnapshot is null)
            {
                return new CheckoutResult(false, Guid.Empty, [], null, null,
                    $"Failed to retrieve pickup address for shop {shopId}: {pickupAddressResult.FailureReason}");
            }

            shopPickupAddresses[shopId] = pickupAddressResult.PickupAddressSnapshot;
            shopNames[shopId] = pickupAddressResult.ShopName;

            List<ShippingFeeItem> shippingItems =
            [
                .. items.Select(i =>
                {
                    ProductInfo product = productInfoMap[priceMap[i.CombinationId].ProductId];
                    return new ShippingFeeItem(i.Quantity, product.WeightGram, product.Length, product.Width,
                        product.Height);
                })
            ];

            ShippingFeeResult feeResult = await shippingClient.CalculateFeeAsync(
                new ShippingFeeRequest(
                    info.CarrierCode,
                    pickupAddressResult.PickupAddressSnapshot.Province,
                    pickupAddressResult.PickupAddressSnapshot.Ward,
                    deliveryAddress.Province,
                    deliveryAddress.Ward,
                    shippingItems),
                cancellationToken);
            if (!feeResult.IsValid)
            {
                return new CheckoutResult(false, Guid.Empty, [], null, null,
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
                    return new CheckoutResult(false, Guid.Empty, [], null, null,
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
                return new CheckoutResult(false, Guid.Empty, [], null, null,
                    $"Voucher platform invalid: {dryRun.FailureReason}");
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
                ShopName = shopNames[shopId],
                CheckoutBatchId = checkoutBatchId,
                Status = OrderStatus.PendingPayment,
                MerchandiseSubtotal = subtotal,
                ShippingFee = shippingFee,
                VoucherDiscount = totalDiscount,
                XuDiscount = 0,
                TotalPayment = totalPayment,
                ShippingAddressSnapshot = deliveryAddress,
                Note = info.Note,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Orders.Add(order);

            orderIds.Add(order.Id);

            foreach (CheckoutItem item in items)
            {
                CombinationPriceInfo combinationInfo = priceMap[item.CombinationId];
                ProductInfo product = productInfoMap[combinationInfo.ProductId];
                context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = combinationInfo.ProductId,
                    CombinationId = item.CombinationId,
                    ProductName = product.ProductName,
                    ThumbnailUrl = product.ThumbnailUrl,
                    Variation = item.Variation,
                    Quantity = item.Quantity,
                    Price = combinationInfo.Price,
                    OriginalPrice = null
                });
            }

            (Guid CarrierId, string CarrierName) carrier = Carriers[info.CarrierCode];

            context.OrderShippingSnapshots.Add(new OrderShippingSnapshot
            {
                OrderId = order.Id,
                CarrierId = carrier.CarrierId,
                CarrierName = carrier.CarrierName,
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

            orderShares.Add(new OrderPaymentShare(order.Id, shopId, totalPayment));

            reserveCommandsPerOrder.Add(new ReserveOrderStock(
                checkoutBatchId, order.Id,
                [.. items.Select(i => new OrderReadyItem(i.CombinationId, i.Quantity))],
                Carriers[info.CarrierCode].Id,
                AddressMapper.ToCheckoutAddressSnapshot(shopPickupAddresses[shopId]),
                AddressMapper.ToCheckoutAddressSnapshot(deliveryAddress)
            ));
        }

        int totalAmount = orderShares.Sum(s => s.Amount);

        outboxWriter.Enqueue(new CheckoutInitiated(
            checkoutBatchId, buyerId, orderIds, command.PaymentMethod, totalAmount,
            orderShares, command.PlatformVoucherCode, shopVouchers));

        foreach (ReserveOrderStock reserveCommand in reserveCommandsPerOrder)
        {
            outboxWriter.Enqueue(reserveCommand);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CheckoutResult(true, checkoutBatchId, orderIds, totalAmount, null, null);
    }
}
