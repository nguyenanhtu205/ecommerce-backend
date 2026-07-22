using System.Security.Cryptography;
using AuthService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.API.Infrastructure;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        JwtOptions jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()!;

        RsaKeyProvider rsaKeyProvider = new(Options.Create(jwtOptions));
        RSA publicKey = rsaKeyProvider.GetPublicKey();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(publicKey)
                };
            });

        return services;
    }
}
