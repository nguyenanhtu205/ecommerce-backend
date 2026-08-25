using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace ShippingService.Infrastructure.Carriers;

public class GhtkOptions
{
    public required string BaseUrl { get; init; }
    public required string Token { get; init; }
    public required string PartnerCode { get; init; }
}

public class GhtkCarrierAdapter(HttpClient httpClient, IOptions<GhtkOptions> options) : ICarrierShippingAdapter
{
    private readonly GhtkOptions _options = options.Value;

    public string CarrierCode => "ghtk";

    public async Task<CarrierFeeResult> CalculateFeeAsync(CarrierShippingRequest request,
        CancellationToken cancellationToken)
    {
        Dictionary<string, string?> query = new()
        {
            ["pick_province"] = request.PickupProvince,
            ["pick_district"] = request.PickupWard,
            ["province"] = request.DeliveryProvince,
            ["district"] = request.DeliveryWard,
            ["weight"] = request.WeightGram.ToString(),
            ["value"] = request.InsuranceValue.ToString(),
            ["transport"] = "road"
        };
        string url = QueryHelpers.AddQueryString("/services/shipment/fee", query);

        using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
        httpRequest.Headers.Add("Token", options.Value.Token);
        httpRequest.Headers.Add("X-Client-Source", options.Value.PartnerCode);

        HttpResponseMessage response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new CarrierFeeResult(false, 0, null, null, $"GHTK fee API trả về {response.StatusCode}");
        }

        GhtkFeeResponse? result = await response.Content.ReadFromJsonAsync<GhtkFeeResponse>(cancellationToken);
        if (result is null || !result.Success || result.Fee is null)
        {
            return new CarrierFeeResult(false, 0, null, null, "GHTK fee API trả dữ liệu không hợp lệ");
        }

        return new CarrierFeeResult(
            true, result.Fee.TotalValue,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            null);
    }

    public async Task<CarrierCreateOrderResult> CreateOrderAsync(CarrierCreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var body = new
        {
            products = new[] { new { name = "Đơn hàng", weight = request.WeightGram / 1000.0, quantity = 1 } },
            order = new
            {
                id = request.OrderId.ToString(),
                pick_name = request.Pickup.FullName,
                pick_address = request.Pickup.FullAddressText,
                pick_province = request.Pickup.Province,
                pick_ward = request.Pickup.Ward,
                pick_tel = request.Pickup.Phone,
                name = request.Delivery.FullName,
                address = request.Delivery.FullAddressText,
                province = request.Delivery.Province,
                ward = request.Delivery.Ward,
                tel = request.Delivery.Phone,
                is_freeship = "1",
                pick_money = 0,
                note = request.Note,
                value = request.InsuranceValue
            }
        };

        using HttpRequestMessage httpRequest = new(HttpMethod.Post, "/services/shipments");
        httpRequest.Headers.Add("Token", _options.Token);
        httpRequest.Headers.Add("X-Client-Source", _options.PartnerCode);
        httpRequest.Content = JsonContent.Create(body);

        HttpResponseMessage response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new CarrierCreateOrderResult(false, null, null, null,
                $"GHTK submit-order trả về {response.StatusCode}");
        }

        GhtkCreateOrderResponse? result =
            await response.Content.ReadFromJsonAsync<GhtkCreateOrderResponse>(cancellationToken);
        if (result is null || !result.Success || result.Order is null)
        {
            return new CarrierCreateOrderResult(false, null, null, null, "GHTK submit-order trả dữ liệu không hợp lệ");
        }

        return new CarrierCreateOrderResult(
            true, result.Order.Label,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            null);
    }

    private record GhtkFeeResponse(
        [property: JsonPropertyName("success")]
        bool Success,
        [property: JsonPropertyName("fee")] GhtkFeeData? Fee);

    private record GhtkFeeData(
        [property: JsonPropertyName("total_value")]
        int TotalValue);

    private record GhtkCreateOrderResponse(
        [property: JsonPropertyName("success")]
        bool Success,
        [property: JsonPropertyName("order")] GhtkOrderData? Order);

    private record GhtkOrderData([property: JsonPropertyName("label")] string Label);
}
