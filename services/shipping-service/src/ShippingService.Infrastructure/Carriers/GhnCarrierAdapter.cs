using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace ShippingService.Infrastructure.Carriers;

public class GhnOptions
{
    public required string BaseUrl { get; init; }
    public required string Token { get; init; }
    public required string ShopId { get; init; }
    public int ServiceTypeId { get; init; } = 2;
}

public class GhnCarrierAdapter(HttpClient httpClient, IOptions<GhnOptions> options) : ICarrierShippingAdapter
{
    private readonly GhnOptions _options = options.Value;

    public string CarrierCode => "ghn";

    public Task<CarrierFeeResult> CalculateFeeAsync(CarrierShippingRequest request, CancellationToken cancellationToken)
    {
        // TODO: GHN /v2/shipping-order/fee bắt buộc from_district_id/to_district_id/
        // to_ward_code (mã số nội bộ GHN, không nhận tên tỉnh/phường dạng text) — cần build
        // bảng map Province+Ward -> mã GHN qua master-data API của họ trước khi gọi thật.
        // Tạm dùng công thức ước tính để không chặn happy path.
        int volumeWeight = request.Length * request.Width * request.Height / 5; 
        int chargeableWeight = Math.Max(request.WeightGram, volumeWeight);
        int fee = request.PickupProvince == request.DeliveryProvince ? 20000 : 30000;
        fee += chargeableWeight / 1000 * 2500;

        return Task.FromResult(new CarrierFeeResult(
            true, fee,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            null));
    }

    public async Task<CarrierCreateOrderResult> CreateOrderAsync(CarrierCreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var body = new
        {
            payment_type_id = 1, // 1 = shop trả phí ship (đã thu qua total_payment lúc checkout)
            required_note = "KHONGCHOXEMHANG",
            to_name = request.Delivery.FullName,
            to_phone = request.Delivery.Phone,
            to_address = request.Delivery.FullAddressText,
            to_ward_name = request.Delivery.Ward,
            to_province_name = request.Delivery.Province,
            from_name = request.Pickup.FullName,
            from_phone = request.Pickup.Phone,
            from_address = request.Pickup.FullAddressText,
            weight = request.WeightGram,
            insurance_value = request.InsuranceValue,
            service_type_id = _options.ServiceTypeId,
            client_order_code = request.OrderId.ToString()
        };

        using HttpRequestMessage httpRequest = new(HttpMethod.Post, "/v2/shipping-order/create");
        httpRequest.Headers.Add("Token", _options.Token);
        httpRequest.Headers.Add("ShopId", _options.ShopId);
        httpRequest.Content = JsonContent.Create(body);

        HttpResponseMessage response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new CarrierCreateOrderResult(false, null, null, null,
                $"GHN create-order trả về {response.StatusCode}");
        }

        GhnCreateOrderResponse? result =
            await response.Content.ReadFromJsonAsync<GhnCreateOrderResponse>(cancellationToken);
        if (result?.Data is null)
        {
            return new CarrierCreateOrderResult(false, null, null, null, "GHN create-order trả dữ liệu rỗng");
        }

        return new CarrierCreateOrderResult(
            true, result.Data.OrderCode,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            null);
    }

    private record GhnFeeResponse([property: JsonPropertyName("data")] GhnFeeData? Data);

    private record GhnFeeData([property: JsonPropertyName("total")] int Total);

    private record GhnCreateOrderResponse([property: JsonPropertyName("data")] GhnCreateOrderData? Data);

    private record GhnCreateOrderData(
        [property: JsonPropertyName("order_code")]
        string OrderCode);
}
