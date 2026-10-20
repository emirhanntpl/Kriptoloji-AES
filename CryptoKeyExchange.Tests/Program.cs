using System.Security.Cryptography;
using System.Text;
using CryptoKeyExchange;

var tests = new (string Name, Action Run)[]
{
    ("GenerateKeys creates a working RSA-2048 key pair", () =>
    {
        using RSA rsa = new RsaKeyExchange().GenerateKeys();
        Assert(rsa.KeySize == 2048, "Expected a 2048-bit key.");
        byte[] message = Encoding.UTF8.GetBytes("RSA anahtar çifti testi");
        byte[] ciphertext = rsa.Encrypt(message, RSAEncryptionPadding.OaepSHA256);
        Assert(rsa.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA256).SequenceEqual(message),
            "The generated private key must decrypt the encrypted message.");
    }),
    ("GenerateKeys creates independent keys", () =>
    {
        var exchange = new RsaKeyExchange();
        using RSA first = exchange.GenerateKeys();
        using RSA second = exchange.GenerateKeys();
        Assert(!first.ExportSubjectPublicKeyInfo().SequenceEqual(second.ExportSubjectPublicKeyInfo()),
            "Each call must generate a new key pair.");
    }),
    ("ExportPublicKey produces an importable public-only PEM key", () =>
    {
        var exchange = new RsaKeyExchange();
        using RSA original = exchange.GenerateKeys();
        string pem = exchange.ExportPublicKey(original);
        Assert(pem.StartsWith("-----BEGIN PUBLIC KEY-----") && pem.EndsWith("-----END PUBLIC KEY-----"),
            "Expected SubjectPublicKeyInfo PEM boundaries.");
        using RSA imported = RSA.Create();
        imported.ImportFromPem(pem);
        Assert(imported.KeySize == 2048, "The exported key must remain 2048 bits.");
        Assert(imported.ExportSubjectPublicKeyInfo().SequenceEqual(original.ExportSubjectPublicKeyInfo()),
            "The exported key must match the original public key.");
        byte[] message = Encoding.UTF8.GetBytes("Dışa aktarılan anahtar testi");
        byte[] ciphertext = imported.Encrypt(message, RSAEncryptionPadding.OaepSHA256);
        Assert(original.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA256).SequenceEqual(message),
            "Messages encrypted with the exported key must be decryptable by the original key.");
        AssertThrows<CryptographicException>(() => imported.ExportParameters(true));
    }),
    ("ExportPublicKey rejects null", () =>
    {
        AssertThrows<ArgumentNullException>(() => new RsaKeyExchange().ExportPublicKey(null!));
    })
};

foreach (var test in tests)
{
    test.Run();
    Console.WriteLine($"PASS: {test.Name}");
}
Console.WriteLine($"All {tests.Length} tests passed.");

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static void AssertThrows<TException>(Action action) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }
    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}
