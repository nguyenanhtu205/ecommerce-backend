namespace AuthService.Infrastructure.Services;

public interface IRsaKeyProvider
{
    string KeyId { get; }
    RSA GetPrivateKey();
    RSA GetPublicKey();
}

public class RsaKeyProvider : IRsaKeyProvider, IDisposable
{
    private readonly RSA _rsa;

    public RsaKeyProvider(IOptions<JwtOptions> options)
    {
        JwtOptions jwtOptions = options.Value;
        KeyId = jwtOptions.KeyId;

        string pem = File.ReadAllText(jwtOptions.PrivateKeyPath);

        _rsa = RSA.Create();
        _rsa.ImportFromPem(pem);
    }

    public void Dispose()
    {
        _rsa.Dispose();
    }

    public string KeyId { get; }

    public RSA GetPrivateKey()
    {
        return _rsa;
    }

    public RSA GetPublicKey()
    {
        RSAParameters publicParams = _rsa.ExportParameters(false);
        RSA publicOnly = RSA.Create();
        publicOnly.ImportParameters(publicParams);
        return publicOnly;
    }
}
