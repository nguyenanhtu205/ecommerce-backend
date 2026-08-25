namespace AuthService.Application.Common.Interfaces;

public interface IOAuthStateStore
{
    Task SetStateAsync(string state, TimeSpan ttl, CancellationToken cancellationToken);

    Task<bool> ConsumeStateAsync(string state, CancellationToken cancellationToken);
}
