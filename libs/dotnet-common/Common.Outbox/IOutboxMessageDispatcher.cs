using System.Text.Json;
using MassTransit;

namespace Common.Outbox;

public interface IOutboxMessageDispatcher
{
    string MessageType { get; }

    Task DispatchAsync(string payload, CancellationToken cancellationToken);
}

public class OutboxMessageDispatcher<T>(ITopicProducer<T> producer) : IOutboxMessageDispatcher where T : class
{
    public string MessageType => typeof(T).FullName!;

    public Task DispatchAsync(string payload, CancellationToken cancellationToken)
    {
        T message = JsonSerializer.Deserialize<T>(payload)!;
        return producer.Produce(message, cancellationToken);
    }
}
