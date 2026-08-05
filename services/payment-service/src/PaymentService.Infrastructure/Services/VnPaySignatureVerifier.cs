using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace PaymentService.Infrastructure.Services;

public class VnPaySignatureVerifier(IOptions<VnPayOptions> options) : IVnPaySignatureVerifier
{
    public bool Verify(IReadOnlyDictionary<string, string> queryParameters, string receivedHash)
    {
        SortedDictionary<string, string> parameters = new(StringComparer.Ordinal);
        foreach ((string key, string value) in queryParameters)
        {
            if (key.StartsWith("vnp_", StringComparison.Ordinal)
                && key is not ("vnp_SecureHash" or "vnp_SecureHashType")
                && !string.IsNullOrEmpty(value))
            {
                parameters[key] = value;
            }
        }

        string queryString = string.Join("&",
            parameters.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        byte[] keyBytes = Encoding.UTF8.GetBytes(options.Value.HashSecret);
        byte[] dataBytes = Encoding.UTF8.GetBytes(queryString);
        string computedHash = Convert.ToHexString(HMACSHA512.HashData(keyBytes, dataBytes)).ToLowerInvariant();

        return string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
    }
}
