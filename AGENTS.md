# Agent çalışma rehberi

## Proje haritası
- C# / .NET 10 sohbet ve kriptoloji projesi.
- `ChatClient/`: `net10.0-windows`, Windows Forms. Giriş: `Program.cs`; arayüz: `MainForm.cs` (`Form1`).
- `ChatClient/Services/NetworkService.cs`: TCP bağlantısı, gönderme/alma; 4 bayt uzunluk öneki + UTF-8 JSON.
- `ChatClient/Models/NetworkPacket.cs`: paket modeli; şu anda boş. `Services/FileServices.cs` de boş `TcpChatClient` sınıfı içeriyor; dosya adından işlev varsayma.
- `CryptoKeyExchange/`: `net10.0` kitaplığı; `RsaKeyExchange.cs` RSA anahtarı üretir ve açık anahtarı PEM olarak dışa aktarır.
- `ChatServer/SecureChatServer.cs`: sunucu kaynağı; henüz `.csproj` dosyası ve solution üyeliği yok.
- `cryptology.sln` ve `ChatClient.slnx`: istemci ve kriptoloji kitaplığını içerir.

## Verimli çalışma
- Kullanıcıyla Türkçe, kısa ve somut iletişim kur. Sonuçta değişikliği, doğrulamayı ve varsa engeli belirt.
- Önce `git status --short` ile mevcut değişiklikleri gör; kullanıcı değişikliklerini koru.
- Önce bu haritadan ilgili dosyayı seç. Aramada `rg -n` ve `rg --files` kullan; yalnızca gereken dosya/bölümleri oku.
- `bin/`, `obj/`, `.git/` ve üretilmiş dosyaları içerik taramasına katma. Büyük çıktıları ve aynı dosyayı gereksiz tekrar okumayı önle.
- Bağımsız okumaları tek çağrıda birleştir; küçük işler için uzun plan veya gereksiz araç çağrısı üretme.
- İstenen işi en küçük tutarlı değişiklikle tamamla. İlgisiz refactor, formatlama, bağımlılık ve dosya ekleme.
- Mevcut C# stilini izle; nullable türleri koru. Arayüz davranışını `MainForm.cs` içinde düzenle; Designer/resx dosyalarına yalnızca arayüz yapısı gerektiriyorsa dokun.
- Kriptografide `System.Security.Cryptography` kullan; anahtarları ve hassas verileri loglama. Ağ/protokol değişikliklerinde istemci ve sunucu uyumunu kontrol et.
- Bu harita başlangıç bilgisidir; işlem öncesi ilgili kaynaktan doğrula. Kalıcı yapı/komut değişirse bu dosyayı kısa tutarak güncelle; oturum geçmişi ekleme.

## Doğrulama
- Kitaplık: `dotnet build CryptoKeyExchange/CryptoKeyExchange.csproj`
- İstemci: `dotnet build ChatClient/ChatClient.csproj`
- İkisi de etkileniyorsa: `dotnet build cryptology.sln`
- İstemcide `EnableWindowsTargeting` açık; Linux'ta derleme hedeflenebilir ancak Windows Forms arayüzü Windows'ta çalıştırılmalıdır.
- Ağ erişimi veya hedefleme paketleri eksikse engeli açıkça bildir; başarısız derlemeyi başarılı sayma.
- Test çalıştırmadan önce mevcut test projelerini bul; olmayan bir test projesi veya komutu varsayma.
- Davranış değişikliğinde ilgili testi çalıştır/ekle; yalnızca belge değişikliğinde derleme gerekmez.
- Son olarak `git diff --check` ve değişen dosyaların diff'ini kontrol et. Kullanıcı istemedikçe commit/push yapma.

## Güvenlik ve mimari
- RSA, DH, ECDH ve ML-KEM yöntemlerini birbirinden ayır;
  seçilmemiş protokol tasarımını varsayma.
- RSA public key aktarımı ile güvenli ve kimliği
  doğrulanmış anahtar değişimini aynı şey sayma.
- Standart kriptografi kütüphanelerini tercih et.
  Manuel AES uygulamasını eğitim amacıyla ayrı tut.
- Private key, shared secret ve AES anahtarlarını loglama.
- Ağ paketlerinde mevcut protokol sözleşmesini koru.

## Ekip ve çalışma sınırları
- Yalnızca istenen modül ve dosyalarda çalış.
- Diğer ekip üyelerinin kodunu gereksiz değiştirme.
- Public API değişikliklerinin etkisini açıkla.
- Önemli kod değişikliklerinin mantığını kısa açıkla.
- Mevcut testleri çalıştır; test altyapısı yoksa
  gereksiz yeni test projesi oluşturma.