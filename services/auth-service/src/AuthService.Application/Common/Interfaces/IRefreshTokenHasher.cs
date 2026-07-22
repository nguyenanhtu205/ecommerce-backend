namespace AuthService.Application.Common.Interfaces;

public interface IRefreshTokenHasher
{
    string Hash(string token);

    bool Verify(string token, string hash);
}
