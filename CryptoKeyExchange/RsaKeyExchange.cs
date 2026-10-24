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
    /// RSA açık anahtarını dışarı aktarır.
    /// </summary>
    public string ExportPublicKey(RSA rsa)
    {
        ArgumentNullException.ThrowIfNull(rsa);
        return rsa.ExportSubjectPublicKeyInfoPem();
    }

    /// <summary>
    /// 16 baytlık AES anahtarını alıcının RSA açık anahtarıyla OAEP/SHA-256 kullanarak şifreler.
    /// </summary>
    public byte[] EncryptKey(byte[] aesKey, RSA recipientPublicKey)
    {
        ArgumentNullException.ThrowIfNull(aesKey);
        ArgumentNullException.ThrowIfNull(recipientPublicKey);

        if (aesKey.Length != 16)
        {
            throw new ArgumentException("AES anahtarı 16 bayt olmalıdır.", nameof(aesKey));
        }

        return recipientPublicKey.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
    }

    /// <summary>
    /// Şifreli AES anahtarını alıcının RSA özel anahtarıyla OAEP/SHA-256 kullanarak çözer.
    /// </summary>
    public byte[] DecryptKey(byte[] encryptedKey, RSA recipientPrivateKey)
    {
        ArgumentNullException.ThrowIfNull(encryptedKey);
        ArgumentNullException.ThrowIfNull(recipientPrivateKey);

        return recipientPrivateKey.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
    }
}
