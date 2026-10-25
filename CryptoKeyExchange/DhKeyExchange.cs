using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace CryptoKeyExchange;

/// <summary>
/// RFC 7919 ffdhe3072 grubuyla klasik Diffie-Hellman anahtar değişimi yapar.
/// Her oturum için yeni bir örnek oluşturulmalıdır. Karşı tarafın kimliği ayrıca doğrulanmalıdır.
/// </summary>
public sealed class DhKeyExchange
{
    public const int PublicKeySize = 384;
    public const int SaltSize = 32;
    public const int KeySize = 16;

    private static readonly DHParameters Parameters = DHStandardGroups.rfc7919_ffdhe3072;

    private readonly DHPrivateKeyParameters privateKey;
    private readonly DHPublicKeyParameters publicKey;

    public DhKeyExchange()
    {
        var generator = new DHKeyPairGenerator();
        generator.Init(new DHKeyGenerationParameters(new SecureRandom(), Parameters));

        var keyPair = generator.GenerateKeyPair();
        privateKey = (DHPrivateKeyParameters)keyPair.Private;
        publicKey = (DHPublicKeyParameters)keyPair.Public;
    }

    /// <summary>
    /// Açık anahtarı sabit uzunlukta, işaretsiz, büyük uçlu 384 bayt olarak dışarı aktarır.
    /// </summary>
    public byte[] ExportPublicKey()
    {
        return BigIntegers.AsUnsignedByteArray(PublicKeySize, publicKey.Y);
    }

    /// <summary>
    /// Karşı tarafın açık anahtarını doğrular ve HKDF-SHA256 ile 16 baytlık AES anahtarı türetir.
    /// İki taraf aynı 32 baytlık salt değerini kullanmalıdır. Salt gizli değildir;
    /// her oturumda RandomNumberGenerator.GetBytes(SaltSize) ile oluşturulup paylaşılmalıdır.
    /// Açık anahtar ve salt aktarımı kimliği doğrulanmış bir kanaldan yapılmalıdır.
    /// </summary>
    public byte[] DeriveKey(byte[] peerPublicKey, byte[] salt)
    {
        ArgumentNullException.ThrowIfNull(peerPublicKey);
        ArgumentNullException.ThrowIfNull(salt);

        if (peerPublicKey.Length != PublicKeySize)
        {
            throw new ArgumentException("DH açık anahtarı 384 bayt olmalıdır.", nameof(peerPublicKey));
        }

        if (salt.Length != SaltSize)
        {
            throw new ArgumentException("Salt 32 bayt olmalıdır.", nameof(salt));
        }

        // DHPublicKeyParameters hem 2..p-2 aralığını hem q alt grubuna üyeliği doğrular.
        var peerKey = new DHPublicKeyParameters(new BigInteger(1, peerPublicKey), Parameters);
        var agreement = new DHBasicAgreement();
        agreement.Init(privateKey);

        byte[] sharedSecret = BigIntegers.AsUnsignedByteArray(
            PublicKeySize, agreement.CalculateAgreement(peerKey));

        try
        {
            return HKDF.DeriveKey(HashAlgorithmName.SHA256, sharedSecret, KeySize, salt,
                "CryptoKeyExchange/DH/ffdhe3072/HKDF-SHA256/AES-128/v1"u8.ToArray());
        }
        finally
        {
            CryptographicOperations.ZeroMemory(sharedSecret);
        }
    }
}
