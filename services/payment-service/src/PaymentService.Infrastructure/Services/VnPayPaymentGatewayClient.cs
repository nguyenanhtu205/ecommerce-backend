using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace PaymentService.Infrastructure.Services;

public class VnPayOptions
{
    public required string TmnCode { get; init; }
    public required string HashSecret { get; init; }
    public required string PaymentBaseUrl { get; init; }
    public required string ReturnUrl { get; init; }
    public string Version { get; init; } = "2.1.0";
    public string Locale { get; init; } = "vn";
    public string CurrCode { get; init; } = "VND";
}

public class VnPayPaymentGatewayClient(IOptions<VnPayOptions> options, IHttpContextAccessor httpContextAccessor)
    : IPaymentGatewayClient
{
    private readonly VnPayOptions _options = options.Value;

    public Task<string> CreateRedirectUrlAsync(Guid checkoutBatchId, int amount, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        string txnRef = checkoutBatchId.ToString("N");

        SortedDictionary<string, string> parameters = new(StringComparer.Ordinal)
        {
            ["vnp_Version"] = _options.Version,
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = (amount * 100).ToString(CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = _options.CurrCode,
            ["vnp_TxnRef"] = txnRef,
            ["vnp_OrderInfo"] = $"Thanh toan don hang {txnRef}",
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = _options.Locale,
            ["vnp_ReturnUrl"] = _options.ReturnUrl,
            ["vnp_IpAddr"] = ipAddress,
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),
            ["vnp_ExpireDate"] = now.AddMinutes(15).ToString("yyyyMMddHHmmss")
        };

        string queryString = BuildQueryString(parameters);
        string secureHash = HmacSha512(_options.HashSecret, queryString);

        string redirectUrl = $"{_options.PaymentBaseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        return Task.FromResult(redirectUrl);
    }

    private static string BuildQueryString(SortedDictionary<string, string> parameters)
    {
        IEnumerable<string> parts = parameters
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}");
        return string.Join("&", parts);
    }

    private static string HmacSha512(string key, string data)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] hashBytes = HMACSHA512.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
