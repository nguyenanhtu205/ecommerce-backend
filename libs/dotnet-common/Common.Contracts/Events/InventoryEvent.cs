namespace Common.Contracts.Events;

public record StockReserved(Guid OrderId);

public record StockReservationFailed(Guid OrderId, string Reason);

public record StockCommitedItem(Guid ProductId, Guid CombinationId, int Quantity);

public record StockCommited(List<StockCommitedItem> StockCommitedItems);
