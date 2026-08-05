using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InventoryService.API.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            string? token = httpContextAccessor.HttpContext?
                .Request
                .Headers["auth"]
                .ToString();

            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            JwtSecurityTokenHandler handler = new();

            if (!handler.CanReadToken(token))
            {
                return null;
            }

            JwtSecurityToken jwt = handler.ReadJwtToken(token);

            string? sub = jwt.Claims
                .FirstOrDefault(x => x.Type == "sub")
                ?.Value;

            return Guid.TryParse(sub, out Guid userId)
                ? userId
                : null;
        }
    }
    
    public bool IsSeller
    {
        get
        {
            string? token = httpContextAccessor.HttpContext?
                .Request
                .Headers["auth"]
                .ToString();

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            JwtSecurityTokenHandler handler = new();

            if (!handler.CanReadToken(token))
            {
                return false;
            }

            JwtSecurityToken jwt = handler.ReadJwtToken(token);

            return jwt.Claims.Any(c =>
                c.Type == ClaimTypes.Role &&
                c.Value == "seller");
        }
    }
}

// public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
// {
//     public Guid? UserId
//     {
//         get
//         {
//             string? userId = httpContextAccessor.HttpContext?.Request
//                 .Headers["X-User-Id"].FirstOrDefault();
//
//             return Guid.TryParse(userId, out Guid id) ? id : null;
//         }
//     }
// }
