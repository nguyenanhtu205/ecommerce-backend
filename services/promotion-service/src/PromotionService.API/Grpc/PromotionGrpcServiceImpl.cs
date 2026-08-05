using Common.Contracts.Grpc.Promotion;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PromotionService.Application.Common.Interfaces;
using PromotionService.Domain.Entities;
using PromotionService.Domain.Enums;

namespace PromotionService.API.Grpc;

public class PromotionGrpcServiceImpl(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : PromotionGrpcService.PromotionGrpcServiceBase
{
    public override async Task<DryRunCalculateDiscountResponse> DryRunCalculateDiscount(
        DryRunCalculateDiscountRequest request, ServerCallContext context)
    {
        Voucher? voucher = await dbContext.Vouchers
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Code == request.VoucherCode, context.CancellationToken);

        if (voucher is null)
        {
            return Invalid("Voucher does not exist.");
        }

        Guid? shopId = request.HasShopId ? Guid.Parse(request.ShopId) : null;

        bool isPlatformVoucher = voucher.Scope == VoucherScope.Platform;
        if (isPlatformVoucher && shopId.HasValue)
        {
            return Invalid("Platform voucher cannot be applied at the shop level.");
        }

        if (!isPlatformVoucher && (!shopId.HasValue || voucher.ShopId != shopId))
        {
            return Invalid("Voucher does not belong to this shop.");
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        if (now < voucher.StartsAt || now > voucher.EndsAt)
        {
            return Invalid("Voucher has expired or is not yet active.");
        }

        if (request.ApplicableAmount < voucher.MinOrderValue)
        {
            return Invalid($"Order does not meet the minimum order value {voucher.MinOrderValue}");
        }

        if (voucher.QuantityLimit.HasValue && voucher.QuantityUsed >= voucher.QuantityLimit.Value)
        {
            return Invalid("Voucher usage limit has been reached.");
        }

        int discount = voucher.DiscountAmount
                       ?? (int)Math.Round(request.ApplicableAmount * (voucher.DiscountPercent ?? 0) / 100m);

        if (voucher.MaxDiscountAmount.HasValue)
        {
            discount = Math.Min(discount, voucher.MaxDiscountAmount.Value);
        }

        discount = Math.Min(discount, request.ApplicableAmount);

        return new DryRunCalculateDiscountResponse { IsValid = true, DiscountAmount = discount };
    }

    private static DryRunCalculateDiscountResponse Invalid(string reason)
    {
        return new DryRunCalculateDiscountResponse { IsValid = false, DiscountAmount = 0, FailureReason = reason };
    }
}
