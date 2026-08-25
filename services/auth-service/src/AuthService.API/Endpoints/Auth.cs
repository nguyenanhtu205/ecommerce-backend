using AuthService.Application.Features.Commands.Login.LoginWithGoogle;
using AuthService.Application.Features.Commands.Login.LoginWithPassword;
using AuthService.Application.Features.Commands.Logout;
using AuthService.Application.Features.Commands.RefreshUserToken;
using AuthService.Application.Features.Commands.Register.RequestOtp;
using AuthService.Application.Features.Commands.Register.SetPassword;
using AuthService.Application.Features.Commands.Register.VerifyOtp;
using AuthService.Application.Features.Queries.GetGoogleAuthorizationUrl;
using AuthService.Application.Features.Queries.GetUser;
using MediatR;

namespace AuthService.API.Endpoints;

public record AccessTokenResponse(string AccessToken);

public class Auth : IEndpointGroup
{
    public static string RoutePrefix => "/auth";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Login, "login")
            .Produces<AccessTokenResponse>()
            .RequireRateLimiting("post");

        groupBuilder.MapPost(Logout, "logout")
            .RequireRateLimiting("post");

        groupBuilder.MapPost(RefreshUserToken, "refresh-user-token")
            .Produces<AccessTokenResponse>()
            .RequireRateLimiting("post");

        groupBuilder.MapPost(RequestOtp, "request-otp")
            .RequireRateLimiting("post");

        groupBuilder.MapPost(VerifyOtp, "verify-otp")
            .RequireRateLimiting("post");

        groupBuilder.MapPost(SetPassword, "set-password")
            .Produces<AccessTokenResponse>()
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetGoogleAuthorizationUrl, "google/authorize-url")
            .Produces<GetGoogleAuthorizationUrlResponse>()
            .RequireRateLimiting("get");

        groupBuilder.MapPost(LoginWithGoogle, "google/login")
            .Produces<AccessTokenResponse>()
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetUser, "user")
            .Produces<GetUserResponse>()
            .RequireRateLimiting("get");
    }

    private static CookieOptions GetRefreshTokenCookieOptions(IWebHostEnvironment env)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };
    }

    [EndpointSummary("Login")]
    public static async Task<IResult> Login(LoginWithPasswordCommand command, ISender sender,
        HttpContext httpContext, IWebHostEnvironment env, CancellationToken cancellationToken)
    {
        LoginWithPasswordResponse result = await sender.Send(command, cancellationToken);

        httpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, GetRefreshTokenCookieOptions(env));

        return Results.Ok(new AccessTokenResponse(result.AccessToken));
    }

    [EndpointSummary("Logout")]
    public static async Task<IResult> Logout(ISender sender,
        HttpContext httpContext, CancellationToken cancellationToken)
    {
        await sender.Send(new LogoutCommand(), cancellationToken);

        httpContext.Response.Cookies.Delete("refreshToken");

        return Results.NoContent();
    }

    [EndpointSummary("Refresh token")]
    public static async Task<IResult> RefreshUserToken(ISender sender,
        HttpContext httpContext, IWebHostEnvironment env, CancellationToken cancellationToken)
    {
        string? refreshToken = httpContext.Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Results.Unauthorized();
        }

        RefreshUserTokenResponse result = await sender.Send(
            new RefreshUserTokenCommand(refreshToken), cancellationToken);

        httpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, GetRefreshTokenCookieOptions(env));

        return Results.Ok(new AccessTokenResponse(result.AccessToken));
    }

    [EndpointSummary("Request OTP")]
    public static async Task<IResult> RequestOtp(
        RequestOtpCommand command, ISender sender, CancellationToken ct)
    {
        await sender.Send(command, ct);
        return Results.NoContent();
    }

    [EndpointSummary("Verify OTP")]
    public static async Task<IResult> VerifyOtp(
        VerifyOtpCommand command, ISender sender, CancellationToken ct)
    {
        await sender.Send(command, ct);
        return Results.NoContent();
    }

    [EndpointSummary("Set password & complete registration")]
    public static async Task<IResult> SetPassword(
        SetPasswordCommand command, ISender sender,
        HttpContext httpContext, IWebHostEnvironment env, CancellationToken ct)
    {
        SetPasswordResponse result = await sender.Send(command, ct);

        httpContext.Response.Cookies.Append(
            "refreshToken", result.RefreshToken, GetRefreshTokenCookieOptions(env));

        return Results.Ok(new AccessTokenResponse(result.AccessToken));
    }

    [EndpointSummary("Get Google OAuth authorization URL")]
    public static async Task<IResult> GetGoogleAuthorizationUrl(ISender sender, CancellationToken cancellationToken)
    {
        GetGoogleAuthorizationUrlResponse result = await sender.Send(
            new GetGoogleAuthorizationUrlQuery(), cancellationToken);

        return Results.Ok(result);
    }

    [EndpointSummary("Login / register via Google")]
    public static async Task<IResult> LoginWithGoogle(LoginWithGoogleCommand command, ISender sender,
        HttpContext httpContext, IWebHostEnvironment env, CancellationToken cancellationToken)
    {
        LoginWithGoogleResponse result = await sender.Send(command, cancellationToken);
        httpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, GetRefreshTokenCookieOptions(env));

        return Results.Ok(new AccessTokenResponse(result.AccessToken));
    }

    [EndpointSummary("Get user")]
    public static async Task<IResult> GetUser(ISender sender, CancellationToken cancellationToken)
    {
        GetUserResponse result = await sender.Send(new GetUserQuery(), cancellationToken);
        return Results.Ok(result);
    }
}
