# Kriptoloji-AES
Kriptoloji.

RSA-2048 anahtar üretimi ve public key dışa aktarma:

```csharp
var exchange = new CryptoKeyExchange.RsaKeyExchange();
using var rsa = exchange.GenerateKeys();
string publicKeyPem = exchange.ExportPublicKey(rsa);
```

`ExportPublicKey` yalnızca public key'i SubjectPublicKeyInfo PEM (`PUBLIC KEY`)
biçiminde döndürür. Üretilen `RSA` nesnesini kullanımdan sonra dispose edin.

Harici test paketi gerektirmeyen testleri çalıştırmak için (.NET 10):

```sh
dotnet run --project CryptoKeyExchange.Tests/CryptoKeyExchange.Tests.csproj
```
