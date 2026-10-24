using System.Security.Cryptography;

namespace CryptoKeyExchange;

public sealed class KeyDerivationService
{
    /// <summary>
    /// Ortak sırdan HKDF-SHA256 ile 16 baytlık AES-128 anahtarı türetir.
    /// İki taraf aynı ortak sır, salt ve info değerlerini kullanmalıdır.
    /// Salt ve info gizli değildir; info kullanım bağlamını ayırmak için kullanılabilir.
    /// Girdi dizileri değiştirilmez. Çağıran, ortak sırrı ve dönen anahtarı kullanım
    /// bittikten sonra CryptographicOperations.ZeroMemory ile silmelidir.
    /// </summary>
    public byte[] DeriveKey(byte[] sharedSecret, byte[]? salt = null, byte[]? info = null)
    {
        ArgumentNullException.ThrowIfNull(sharedSecret);

        if (sharedSecret.Length == 0)
        {
            throw new ArgumentException("Ortak sır boş olamaz.", nameof(sharedSecret));
        }

        return HKDF.DeriveKey(HashAlgorithmName.SHA256, sharedSecret, 16, salt, info);
    }
}
