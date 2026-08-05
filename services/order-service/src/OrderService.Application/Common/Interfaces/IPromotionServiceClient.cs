namespace OrderService.Application.Common.Interfaces;

public interface IPromotionServiceClient
{
    Task<VoucherDryRunResult> DryRunCalculateDiscountAsync(VoucherDryRunRequest request,
        CancellationToken cancellationToken);
}

public record VoucherDryRunRequest(string VoucherCode, Guid? ShopId, Guid BuyerId, int ApplicableAmount);

public record VoucherDryRunResult(bool IsValid, int DiscountAmount, string? FailureReason);
