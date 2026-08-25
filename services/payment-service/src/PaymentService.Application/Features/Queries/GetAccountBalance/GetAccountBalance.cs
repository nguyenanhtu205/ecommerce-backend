namespace PaymentService.Application.Features.Queries.GetAccountBalance;

public record BankAccountSummary(
    Guid Id,
    string BankName,
    string AccountNumberMasked,
    bool IsDefault,
    bool IsVerified);

public record AccountBalanceOverview(
    int Balance,
    int AvailableBalance,
    int PendingBalance,
    int DebtBalance);

public record WalletTransactionItem(
    Guid Id,
    DateTimeOffset CreatedAt,
    WalletTransactionType Type,
    Guid? OrderId,
    string Flow,
    int Amount,
    string Status);

public record GetAccountBalanceResponse(
    AccountBalanceOverview Overview,
    List<WalletTransactionItem> Transactions,
    int TotalCount,
    int TotalAmount);

public record GetAccountBalanceQuery(
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Flow,
    List<WalletTransactionType>? Types,
    string? OrderIdSearch,
    int Page = 1,
    int PageSize = 20)
    : IRequest<GetAccountBalanceResponse>;

public class GetAccountBalance(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetAccountBalanceQuery, GetAccountBalanceResponse>
{
    public async Task<GetAccountBalanceResponse> Handle(
        GetAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid shopId = currentUser.ShopId.Value;

        ShopWallet? wallet = await context.ShopWallets
            .FirstOrDefaultAsync(w => w.ShopId == shopId, cancellationToken);

        AccountBalanceOverview overview = new(
            wallet?.AvailableBalance ?? 0,
            (wallet?.AvailableBalance ?? 0) - (wallet?.DebtBalance ?? 0),
            wallet?.PendingBalance ?? 0,
            wallet?.DebtBalance ?? 0);

        IQueryable<ShopWalletTransaction> query = context.ShopWalletTransactions
            .Where(t => t.ShopId == shopId);

        if (request.From.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.To.Value);
        }

        if (request.Flow == "in")
        {
            query = query.Where(t => t.Amount >= 0);
        }
        else if (request.Flow == "out")
        {
            query = query.Where(t => t.Amount < 0);
        }

        if (request.Types is { Count: > 0 })
        {
            query = query.Where(t => request.Types.Contains(t.Type));
        }

        if (!string.IsNullOrWhiteSpace(request.OrderIdSearch)
            && Guid.TryParse(request.OrderIdSearch, out Guid searchOrderId))
        {
            query = query.Where(t => t.OrderId == searchOrderId);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        int totalAmount = await query.SumAsync(t => t.Amount, cancellationToken);

        List<ShopWalletTransaction> pageItems = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        List<WalletTransactionItem> transactions =
        [
            .. pageItems.Select(t => new WalletTransactionItem(
                t.Id,
                t.CreatedAt,
                t.Type,
                t.OrderId,
                t.Amount >= 0 ? "in" : "out",
                t.Amount,
                "Hoàn thành"))
        ];

        return new GetAccountBalanceResponse(overview, transactions, totalCount, totalAmount);
    }
}
