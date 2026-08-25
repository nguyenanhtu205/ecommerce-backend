using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Web;
using Common.Domain.Exceptions;
using FluentValidation.Results;

namespace AuthService.Infrastructure.Services;

public class GoogleOAuthService(HttpClient httpClient, IOptions<GoogleOAuthOptions> options) : IGoogleOAuthService
{
    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

    private readonly GoogleOAuthOptions _options = options.Value;

    public string GetAuthorizationUrl(string state)
    {
        Dictionary<string, string?> query = new()
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = _options.Scope,
            ["state"] = state,
            ["access_type"] = "offline",
            ["prompt"] = "select_account"
        };

        string queryString = string.Join("&",
            query.Select(kv => $"{kv.Key}={HttpUtility.UrlEncode(kv.Value)}"));

        return $"{AuthorizationEndpoint}?{queryString}";
    }

    public async Task<GoogleUserInfo> ExchangeCodeForUserInfoAsync(string code, CancellationToken cancellationToken)
    {
        Dictionary<string, string> form = new()
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUri,
            ["grant_type"] = "authorization_code"
        };

        using HttpResponseMessage response = await httpClient.PostAsync(
            TokenEndpoint,
            new FormUrlEncodedContent(form),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ValidationException([
                new ValidationFailure("GoogleOAuth", "Failed to exchange authorization code with Google.")
            ]);
        }

        GoogleTokenResponse? tokenResponse = await response.Content
            .ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken);

        if (tokenResponse?.IdToken is null)
        {
            throw new ValidationException([new ValidationFailure("GoogleOAuth", "Google did not return an id_token.")]);
        }

        JwtSecurityToken idToken = new JwtSecurityTokenHandler().ReadJwtToken(tokenResponse.IdToken);

        string? googleUserId = idToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        string? email = idToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        bool emailVerified = bool.TryParse(
            idToken.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value,
            out bool parsed) && parsed;

        if (string.IsNullOrEmpty(googleUserId) || string.IsNullOrEmpty(email))
        {
            throw new ValidationException([
                new ValidationFailure("GoogleOAuth", "Google id_token is missing required claims.")
            ]);
        }

        return new GoogleUserInfo(email, googleUserId, emailVerified);
    }

    private record GoogleTokenResponse(
        [property: JsonPropertyName("access_token")]
        string? AccessToken,
        [property: JsonPropertyName("id_token")]
        string? IdToken,
        [property: JsonPropertyName("expires_in")]
        int ExpiresIn,
        [property: JsonPropertyName("token_type")]
        string? TokenType);
}
