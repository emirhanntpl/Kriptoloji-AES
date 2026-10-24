using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Kems;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace CryptoKeyExchange;

/// <summary>
/// Kyber ailesinden standartlaştırılmış ML-KEM-768 (FIPS 203) uygular; eski Kyber ile uyumlu değildir.
/// Her oturumda KeyGen ile yeni bir alıcı oluşturulmalı ve karşı tarafın kimliği ayrıca doğrulanmalıdır.
/// Bouncy Castle özel anahtar nesnesi deterministik bellek temizleme API'si sunmaz.
/// </summary>
public sealed class KyberKeyExchange
{
    public const int PublicKeySize = 1184;
    public const int CiphertextSize = 1088;
    public const int SharedSecretSize = 32;

    private static readonly MLKemParameters Parameters = MLKemParameters.ml_kem_768;
    private readonly MLKemPrivateKeyParameters privateKey;

    private KyberKeyExchange(MLKemPrivateKeyParameters privateKey)
    {
        this.privateKey = privateKey;
    }

    /// <summary>Yeni anahtar çifti oluşturur; özel anahtar örneğin içinde kalır.</summary>
    public static KyberKeyExchange KeyGen()
    {
        var generator = new MLKemKeyPairGenerator();
        generator.Init(new MLKemKeyGenerationParameters(new SecureRandom(), Parameters));
        return new KyberKeyExchange((MLKemPrivateKeyParameters)generator.GenerateKeyPair().Private);
    }

    /// <summary>1184 baytlık ham ML-KEM-768 açık anahtarının kopyasını döndürür.</summary>
    public byte[] ExportPublicKey()
    {
        return privateKey.GetPublicKeyEncoded();
    }

    /// <summary>
    /// Alıcının açık anahtarından şifreli kapsül ve 32 baytlık ortak sır üretir.
    /// Yalnızca Ciphertext alıcıya gönderilir. SharedSecret, KeyDerivationService ile
    /// AES anahtarına türetilmeli ve kullanım sonunda CryptographicOperations.ZeroMemory ile silinmelidir.
    /// </summary>
    public static (byte[] Ciphertext, byte[] SharedSecret) Encapsulate(byte[] recipientPublicKey)
    {
        ArgumentNullException.ThrowIfNull(recipientPublicKey);
        if (recipientPublicKey.Length != PublicKeySize)
        {
            throw new ArgumentException("ML-KEM-768 açık anahtarı 1184 bayt olmalıdır.", nameof(recipientPublicKey));
        }

        var publicKey = MLKemPublicKeyParameters.FromEncoding(Parameters, recipientPublicKey);
        var encapsulator = new MLKemEncapsulator(Parameters);
        encapsulator.Init(new ParametersWithRandom(publicKey, new SecureRandom()));
        byte[] ciphertext = new byte[CiphertextSize];
        byte[] sharedSecret = new byte[SharedSecretSize];
        try
        {
            encapsulator.Encapsulate(ciphertext, sharedSecret);
            return (ciphertext, sharedSecret);
        }
        catch
        {
            CryptographicOperations.ZeroMemory(sharedSecret);
            throw;
        }
    }

    /// <summary>
    /// 1088 baytlık kapsülden ortak sır üretir. Dönen sır kullanım sonunda silinmelidir.
    /// Doğru uzunlukta fakat değiştirilmiş kapsül, ML-KEM örtük reddi nedeniyle hata yerine
    /// farklı bir sır üretir; mesaj doğrulaması için örneğin AES-GCM kullanılmalıdır.
    /// </summary>
    public byte[] Decapsulate(byte[] ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);
        if (ciphertext.Length != CiphertextSize)
        {
            throw new ArgumentException("ML-KEM-768 kapsülü 1088 bayt olmalıdır.", nameof(ciphertext));
        }

        var decapsulator = new MLKemDecapsulator(Parameters);
        decapsulator.Init(privateKey);
        byte[] sharedSecret = new byte[SharedSecretSize];
        try
        {
            decapsulator.Decapsulate(ciphertext, sharedSecret);
            return sharedSecret;
        }
        catch
        {
            CryptographicOperations.ZeroMemory(sharedSecret);
            throw;
        }
    }
}
