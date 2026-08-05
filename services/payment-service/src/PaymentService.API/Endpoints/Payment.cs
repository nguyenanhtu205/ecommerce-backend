using MediatR;
using PaymentService.Application.Features.Commands.ConfirmVnPayPayment;

namespace PaymentService.API.Endpoints;

public class Payment : IEndpointGroup
{
    public static string RoutePrefix => "/payment";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(HandleIpnAsync, "vnpay/ipn")
            .Produces<ConfirmVnPayPaymentResult>()
            .RequireRateLimiting("post");
    }

    private static async Task<IResult> HandleIpnAsync(
        HttpRequest request, ISender sender, CancellationToken cancellationToken)
    {
        Dictionary<string, string> queryParameters =
            request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        ConfirmVnPayPaymentResult result =
            await sender.Send(new ConfirmVnPayPaymentCommand(queryParameters), cancellationToken);

        return Results.Ok(result);
    }
}
