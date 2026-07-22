namespace AuthService.Infrastructure.Options;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; init; }
}
