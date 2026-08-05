namespace PaymentService.Application.Common.Interfaces;

public interface IVnPaySignatureVerifier
{
    bool Verify(IReadOnlyDictionary<string, string> queryParameters, string receivedHash);
}
