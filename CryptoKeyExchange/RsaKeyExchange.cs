using System.Security.Cryptography;

namespace CryptoKeyExchange;

public class RsaKeyExchange
{
    public RSA GenerateKeys()
    {
        RSA rsa = RSA.Create(2048);
        return rsa;
    }

    /// <summary>
    /// Exports the public key in SubjectPublicKeyInfo PEM format.
    /// </summary>
    public string ExportPublicKey(RSA rsa)
    {
        ArgumentNullException.ThrowIfNull(rsa);
        return rsa.ExportSubjectPublicKeyInfoPem();
    }
}
