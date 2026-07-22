namespace AuthService.Application.Common.Interfaces;

public record OtpVerifyResult(bool IsValid, string? Role);

public interface IOtpStore
{
    Task SetCodeAsync(string email, string plainCode, string role, TimeSpan ttl, CancellationToken ct);

    Task<OtpVerifyResult> VerifyCodeAsync(string email, string plainCode, CancellationToken ct);

    Task MarkVerifiedAsync(string email, string role, TimeSpan ttl, CancellationToken ct);

    Task<string?> GetVerifiedRoleAsync(string email, CancellationToken ct);

    Task ClearVerifiedAsync(string email, CancellationToken ct);
}
