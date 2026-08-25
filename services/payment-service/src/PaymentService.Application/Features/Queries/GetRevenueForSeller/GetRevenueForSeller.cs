namespace PaymentService.Application.Features.Queries.GetRevenueForSeller;

public record RevenueOverview(
    int PendingBalance,
    int PaidThisWeek,
    int PaidThisMonth,
    int PaidTotal,
    int DebtBalance);

public record UnpaidOrderItem(
    Guid OrderId,
    Guid BuyerId,
    PaymentMethodType PaymentMethod,
    int Amount,
    DateTimeOffset ReleaseDueAt);

public record PaidOrderItem(
    Guid OrderId,
    Guid BuyerId,
    PaymentMethodType PaymentMethod,
    int Amount,
    DateTimeOffset PaidAt,
    int RefundedAmount);

public record GetRevenueForSellerResponse(
    RevenueOverview Overview,
    List<UnpaidOrderItem> UnpaidOrders,
    List<PaidOrderItem> PaidOrders);

public record GetRevenueForSellerQuery(DateTimeOffset? PaidFrom, DateTimeOffset? PaidTo)
    : IRequest<GetRevenueForSellerResponse>;

public class GetRevenueForSeller(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetRevenueForSellerQuery, GetRevenueForSellerResponse>
{
    public async Task<GetRevenueForSellerResponse> Handle(GetRevenueForSellerQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid shopId = currentUser.ShopId.Value;

        RevenueOverview overview = await BuildOverviewAsync(shopId, cancellationToken);

        List<UnpaidOrderItem> unpaidOrders = await context.EscrowHolds
            .AsNoTracking()
            .Where(e => e.ShopId == shopId && e.Status == EscrowStatus.Held)
            .Include(e => e.Payment)
            .OrderByDescending(e => e.ReleaseDueAt)
            .Join(context.Payments, e => e.PaymentId, p => p.Id,
                (e, p) => new UnpaidOrderItem(e.OrderId, e.Payment!.BuyerId, p.Method, e.Amount, e.ReleaseDueAt))
            .ToListAsync(cancellationToken);

        IQueryable<EscrowHold> paidQuery = context.EscrowHolds
            .AsNoTracking()
            .Where(e => e.ShopId == shopId && e.Status == EscrowStatus.Released)
            .Include(e => e.Payment);

        if (request.PaidFrom.HasValue)
        {
            paidQuery = paidQuery.Where(e => e.ReleasedAt >= request.PaidFrom.Value);
        }

        if (request.PaidTo.HasValue)
        {
            paidQuery = paidQuery.Where(e => e.ReleasedAt <= request.PaidTo.Value);
        }

        List<EscrowHold> paidEscrows = await paidQuery
            .OrderByDescending(e => e.ReleasedAt)
            .ToListAsync(cancellationToken);

        List<Guid> paidOrderIds = [.. paidEscrows.Select(e => e.OrderId)];
        List<Guid> paidPaymentIds = [.. paidEscrows.Select(e => e.PaymentId).Distinct()];

        Dictionary<Guid, PaymentMethodType> paymentMethodByPaymentId = await context.Payments
            .AsNoTracking()
            .Where(p => paidPaymentIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Method, cancellationToken);

        Dictionary<Guid, int> refundedByOrderId = await context.Refunds
            .AsNoTracking()
            .Where(r => paidOrderIds.Contains(r.OrderId) && r.Status == RefundStatus.Succeeded)
            .GroupBy(r => r.OrderId)
            .Select(g => new { OrderId = g.Key, Total = g.Sum(r => r.Amount) })
            .ToDictionaryAsync(x => x.OrderId, x => x.Total, cancellationToken);

        List<PaidOrderItem> paidOrders =
        [
            .. paidEscrows.Select(e => new PaidOrderItem(
                e.OrderId,
                e.Payment!.BuyerId,
                paymentMethodByPaymentId.GetValueOrDefault(e.PaymentId),
                e.Amount,
                e.ReleasedAt!.Value,
                refundedByOrderId.GetValueOrDefault(e.OrderId)))
        ];

        return new GetRevenueForSellerResponse(overview, unpaidOrders, paidOrders);
    }

    private async Task<RevenueOverview> BuildOverviewAsync(Guid shopId, CancellationToken cancellationToken)
    {
        ShopWallet? wallet = await context.ShopWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.ShopId == shopId, cancellationToken);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset startOfWeek =
            now.Date.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        DateTimeOffset startOfMonth = new(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        IQueryable<ShopWalletTransaction> releaseTransactions = context.ShopWalletTransactions
            .Where(t => t.ShopId == shopId && t.Type == WalletTransactionType.EscrowRelease);

        int paidThisWeek = await releaseTransactions
            .Where(t => t.CreatedAt >= startOfWeek)
            .SumAsync(t => t.Amount, cancellationToken);

        int paidThisMonth = await releaseTransactions
            .Where(t => t.CreatedAt >= startOfMonth)
            .SumAsync(t => t.Amount, cancellationToken);

        int paidTotal = await releaseTransactions.SumAsync(t => t.Amount, cancellationToken);

        return new RevenueOverview(
            wallet?.PendingBalance ?? 0,
            paidThisWeek,
            paidThisMonth,
            paidTotal,
            wallet?.DebtBalance ?? 0);
    }
}
