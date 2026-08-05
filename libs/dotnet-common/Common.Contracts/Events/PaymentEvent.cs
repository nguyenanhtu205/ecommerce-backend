namespace Common.Contracts.Events;

public record PaymentRedirectCreated(Guid CheckoutBatchId, string RedirectUrl);

public record VnPayPaymentConfirmed(Guid CheckoutBatchId, bool Success, string? ProviderTransactionId, string? Reason);
