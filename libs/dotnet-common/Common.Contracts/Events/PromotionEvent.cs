namespace Common.Contracts.Events;

public record VoucherRedeemed(Guid CheckoutBatchId);

public record VoucherRedemptionFailed(Guid CheckoutBatchId, string Reason);
