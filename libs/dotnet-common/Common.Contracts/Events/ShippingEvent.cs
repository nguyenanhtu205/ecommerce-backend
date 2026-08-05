namespace Common.Contracts.Events;

public record ShipmentCreated(Guid OrderId);

public record ShipmentCreationFailed(Guid OrderId, string Reason);

public record OrderDelivered(Guid OrderId, DateTimeOffset DeliveredAt);
