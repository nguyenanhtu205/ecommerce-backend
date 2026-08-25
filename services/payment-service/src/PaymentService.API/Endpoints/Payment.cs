using MediatR;
using PaymentService.Application.Features.Commands.ConfirmVnPayPayment;
using PaymentService.Application.Features.Queries.GetAccountBalance;
using PaymentService.Application.Features.Queries.GetRevenueForSeller;

namespace PaymentService.API.Endpoints;

public class Payment : IEndpointGroup
{
    public static string RoutePrefix => "/payment";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(HandleIpnAsync, "vnpay/ipn")
            .Produces<ConfirmVnPayPaymentResult>()
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetRevenueForSellerAsync, "revenue")
            .Produces<GetRevenueForSellerResponse>()
            .RequireRateLimiting("get");

        groupBuilder.MapPost(GetAccountBalance, "account-balance")
            .Produces<GetAccountBalanceResponse>()
            .RequireRateLimiting("post");
    }

    [EndpointSummary("Vnpay")]
    private static async Task<IResult> HandleIpnAsync(
        HttpRequest request, ISender sender, CancellationToken cancellationToken)
    {
        Dictionary<string, string> queryParameters =
            request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        ConfirmVnPayPaymentResult result =
            await sender.Send(new ConfirmVnPayPaymentCommand(queryParameters), cancellationToken);

        return Results.Ok(result);
    }

    [EndpointSummary("Revenue")]
    private static async Task<IResult> GetRevenueForSellerAsync(
        [AsParameters] GetRevenueForSellerQuery query, ISender sender, CancellationToken cancellationToken)
    {
        GetRevenueForSellerResponse result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Account balance")]
    private static async Task<IResult> GetAccountBalance(GetAccountBalanceQuery query, ISender sender,
        CancellationToken cancellationToken)
    {
        GetAccountBalanceResponse result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
