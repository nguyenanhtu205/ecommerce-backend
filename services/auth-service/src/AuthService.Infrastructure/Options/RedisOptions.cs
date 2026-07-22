namespace AuthService.Infrastructure.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public required string ConnectionString { get; init; }
}
