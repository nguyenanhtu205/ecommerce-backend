namespace PaymentService.Domain.Enums;

public enum WalletTransactionType
{
    EscrowRelease,
    Withdrawal,
    RefundDeduction,
    DebtIncrease,
    DebtSettlement
}
