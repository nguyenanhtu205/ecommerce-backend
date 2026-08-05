namespace Common.Contracts.Events;

public record StockReserved(Guid OrderId);

public record StockReservationFailed(Guid OrderId, string Reason);
