using Common.Contracts.Grpc.Promotion;

namespace OrderService.Infrastructure.Services;

public class PromotionServiceClient(
    PromotionGrpcService.PromotionGrpcServiceClient grpcClient) : IPromotionServiceClient
{
    public async Task<VoucherDryRunResult> DryRunCalculateDiscountAsync(
        VoucherDryRunRequest request, CancellationToken cancellationToken)
    {
        DryRunCalculateDiscountRequest grpcRequest = new()
        {
            VoucherCode = request.VoucherCode,
            BuyerId = request.BuyerId.ToString(),
            ApplicableAmount = request.ApplicableAmount
        };
        if (request.ShopId.HasValue)
        {
            grpcRequest.ShopId = request.ShopId.Value.ToString();
        }

        DryRunCalculateDiscountResponse response = await grpcClient.DryRunCalculateDiscountAsync(
            grpcRequest, deadline: DateTime.UtcNow.AddSeconds(3), cancellationToken: cancellationToken);

        return new VoucherDryRunResult(
            response.IsValid,
            response.DiscountAmount,
            response.HasFailureReason ? response.FailureReason : null);
    }
}
