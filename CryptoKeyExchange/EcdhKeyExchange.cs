using System.Security.Cryptography;

namespace CryptoKeyExchange;

/// <summary>
/// P-256 eğrisiyle ECDH ortak sırrı üretir. Her oturum için yeni bir örnek oluşturulmalıdır.
/// Karşı tarafın kimliği ayrıca doğrulanmalıdır.
/// </summary>
public sealed class EcdhKeyExchange : IDisposable
{
    private readonly ECDiffieHellman key = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
    private bool disposed;

    /// <summary>
    /// Açık anahtarı DER kodlu SubjectPublicKeyInfo biçiminde dışarı aktarır.
    /// </summary>
    public byte[] ExportPublicKey()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return key.ExportSubjectPublicKeyInfo();
    }

    /// <summary>
    /// Karşı tarafın DER kodlu P-256 açık anahtarından 32 baytlık ham ortak sır üretir.
    /// Bu sır doğrudan AES anahtarı olarak kullanılmamalı; önce bir KDF ile anahtar türetilmelidir.
    /// Çağıran, kullanım bittikten sonra dönen diziyi CryptographicOperations.ZeroMemory ile silmelidir.
    /// </summary>
    public byte[] DeriveSharedSecret(byte[] peerPublicKey)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(peerPublicKey);

        using var peer = ECDiffieHellman.Create();
        peer.ImportSubjectPublicKeyInfo(peerPublicKey, out int bytesRead);

        if (bytesRead != peerPublicKey.Length)
        {
            throw new ArgumentException("Açık anahtarın sonunda fazladan veri var.", nameof(peerPublicKey));
        }

        if (peer.ExportParameters(false).Curve.Oid.Value != ECCurve.NamedCurves.nistP256.Oid.Value)
        {
            throw new ArgumentException("ECDH açık anahtarı P-256 eğrisini kullanmalıdır.", nameof(peerPublicKey));
        }

        using ECDiffieHellmanPublicKey peerKey = peer.PublicKey;
        return key.DeriveRawSecretAgreement(peerKey);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        key.Dispose();
        disposed = true;
    }
}
