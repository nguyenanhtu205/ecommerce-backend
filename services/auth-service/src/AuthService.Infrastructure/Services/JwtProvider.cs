using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Services;

public class JwtProvider(
    IOptions<JwtOptions> options,
    IRsaKeyProvider rsaKeyProvider
) : IJwtProvider
{
    private readonly JwtOptions _options = options.Value;

    public string Generate(User user)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            .. user.UserRoles.Select(ur => new Claim("role", ur.Role!.Name))
        ];

        RSA privateKey = rsaKeyProvider.GetPrivateKey();

        RsaSecurityKey securityKey = new(privateKey) { KeyId = rsaKeyProvider.KeyId };

        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.RsaSha256);

        JwtSecurityToken token = new(
            _options.Issuer,
            _options.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),
            signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
