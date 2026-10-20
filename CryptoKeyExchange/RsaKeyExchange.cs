using System.Security.Cryptography;

namespace CryptoKeyExchange;

public class RsaKeyExchange
{
    public RSA GenerateKeys()
    {
        RSA rsa = RSA.Create(2048);
        return rsa;
    }
}