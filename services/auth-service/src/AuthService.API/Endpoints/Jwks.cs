using System.Security.Cryptography;
using AuthService.Infrastructure.Services;

namespace AuthService.API.Endpoints;

public class Jwks : IEndpointGroup
{
    public static string RoutePrefix => "/.well-known";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetJwks, "jwks.json")
            .RequireRateLimiting("get");
    }

    [EndpointSummary("JSON Web Key Set")]
    [EndpointDescription(
        "Returns the public keys used to verify JWTs issued by this service, in JWK Set format (RFC 7517). Used by API gateways such as Kong to validate access tokens without contacting this service for every request.")]
    public static IResult GetJwks(IRsaKeyProvider rsaKeyProvider, HttpContext httpContext)
    {
        RSA publicKey = rsaKeyProvider.GetPublicKey();
        RSAParameters parameters = publicKey.ExportParameters(false);

        var jwks = new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = rsaKeyProvider.KeyId,
                    alg = "RS256",
                    n = Base64UrlEncode(parameters.Modulus!),
                    e = Base64UrlEncode(parameters.Exponent!)
                }
            }
        };

        httpContext.Response.Headers.CacheControl = "public, max-age=3600";

        return Results.Ok(jwks);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
