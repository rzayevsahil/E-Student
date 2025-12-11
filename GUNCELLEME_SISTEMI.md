# 🔄 Otomatik Güncelleme Sistemi Kullanım Kılavuzu

## 📋 Genel Bakış

Uygulamanız artık otomatik güncelleme kontrolü yapabilir. Bu sistem GitHub Releases API kullanarak yeni sürümleri kontrol eder ve kullanıcıya bildirim gösterir.

## 🚀 Nasıl Çalışır?

### 1. **Arka Plan Kontrolü**
- Uygulama her açıldığında otomatik olarak (3 saniye sonra) güncelleme kontrolü yapar
- Eğer yeni sürüm varsa, kullanıcıya bildirim gösterilir
- Eğer güncel ise, sessizce devam eder (bildirim göstermez)

### 2. **Manuel Kontrol**
- Sol menüdeki **"🔄 Güncellemeleri Kontrol Et"** butonuna tıklayarak manuel kontrol yapabilirsiniz

## ⚙️ Kurulum ve Yapılandırma

### Adım 1: GitHub Repository Bilgilerini Güncelle

`DocumentSearch/Services/UpdateService.cs` dosyasını açın ve şu satırları güncelleyin:

```csharp
// TODO: GitHub repository bilgilerinizi buraya ekleyin
_githubRepoOwner = "YOUR_GITHUB_USERNAME"; // GitHub kullanıcı adınız
_githubRepoName = "DocumentSearch"; // Repository adınız
```

**Örnek:**
```csharp
_githubRepoOwner = "sahilrzayev"; // GitHub kullanıcı adınız
_githubRepoName = "DocumentSearch"; // Repository adınız
```

### Adım 2: Sürüm Numarasını Güncelle

Her yeni sürümde `DocumentSearch.csproj` dosyasındaki sürüm numarasını artırın:

```xml
<Version>1.0.0</Version>  <!-- Örnek: 1.0.1, 1.1.0, 2.0.0 vb. -->
```

### Adım 3: GitHub'da Release Oluştur

1. GitHub repository'nize gidin
2. **Releases** sekmesine tıklayın
3. **"Create a new release"** butonuna tıklayın
4. **Tag version** alanına sürüm numarasını girin (örn: `v1.0.1` veya `1.0.1`)
5. **Release title** alanına başlık girin (örn: "Version 1.0.1")
6. **Description** alanına değişiklik notlarını yazın
7. **Binary files** bölümüne yeni `.exe` dosyasını ekleyin (veya `.msi` setup dosyası)
8. **"Publish release"** butonuna tıklayın

## 📦 Güncelleme Dosyası Hazırlama

### Yöntem 1: Setup Dosyası (Önerilen)

1. Projeyi publish edin:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

2. Bir setup/installer oluşturun (örneğin Inno Setup, NSIS, veya ClickOnce)

3. Setup dosyasını GitHub Release'e ekleyin

### Yöntem 2: Tek Dosya Executable

1. Projeyi publish edin:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

2. `bin/Release/net8.0-windows/win-x64/publish/DocumentSearch.exe` dosyasını GitHub Release'e ekleyin

## 🔍 Sürüm Kontrolü Nasıl Yapılır?

### GitHub Releases API

Sistem şu URL'yi kullanarak en son sürümü kontrol eder:
```
https://api.github.com/repos/{OWNER}/{REPO}/releases/latest
```

API'den dönen JSON formatı:
```json
{
  "tag_name": "v1.0.1",
  "assets": [
    {
      "name": "DocumentSearch_Setup.exe",
      "browser_download_url": "https://github.com/.../DocumentSearch_Setup.exe"
    }
  ]
}
```

### Sürüm Karşılaştırması

- Mevcut sürüm: `Assembly.GetExecutingAssembly().GetName().Version` (csproj'deki `<Version>`)
- Yeni sürüm: GitHub'dan gelen `tag_name`
- Eğer yeni sürüm > mevcut sürüm ise, güncelleme bildirimi gösterilir

## 🎯 Kullanıcı Deneyimi

### Senaryo 1: Yeni Sürüm Bulundu

1. Kullanıcıya şu mesaj gösterilir:
   ```
   Yeni bir sürüm mevcut!
   
   Mevcut Sürüm: 1.0.0
   Yeni Sürüm: 1.0.1
   
   Güncellemeyi şimdi indirmek ister misiniz?
   [Evet] [Hayır]
   ```

2. Kullanıcı **"Evet"** derse:
   - İndirme konumu seçilir
   - Dosya indirilir
   - İndirilen dosya otomatik çalıştırılır
   - Uygulama kapanır (güncelleme kurulumu başlatıldı)

3. Kullanıcı **"Hayır"** derse:
   - İşlem iptal edilir
   - Uygulama normal şekilde devam eder

### Senaryo 2: Güncel Sürüm

- Manuel kontrol yapıldığında:
  ```
  Uygulamanız güncel!
  
  Mevcut Sürüm: 1.0.1
  ```

## 🔧 Gelişmiş Özellikler

### Özel Sunucu/API Kullanımı

Eğer GitHub yerine kendi sunucunuzu kullanmak isterseniz, `UpdateService.cs` dosyasındaki `GetLatestVersionAsync()` metodunu değiştirebilirsiniz:

```csharp
private async Task<string?> GetLatestVersionAsync()
{
    // Kendi API endpoint'inizi kullanın
    var response = await _httpClient.GetStringAsync("https://your-api.com/latest-version");
    // JSON parse edin ve sürüm numarasını döndürün
}
```

### AutoUpdater.NET Kütüphanesi (Alternatif)

Daha gelişmiş özellikler için `AutoUpdater.NET` NuGet paketini kullanabilirsiniz:

```bash
dotnet add package AutoUpdater.NET
```

## 📝 Notlar

- **İnternet Bağlantısı**: Güncelleme kontrolü için aktif internet bağlantısı gereklidir
- **GitHub API Limitleri**: GitHub API'si saatte 60 istek limitine sahiptir (anonim kullanıcılar için)
- **Güvenlik**: İndirilen dosyaların güvenliğinden kullanıcı sorumludur
- **Sürüm Formatı**: Sürüm numaraları `X.Y.Z` formatında olmalıdır (örn: 1.0.1, 2.1.0)

## 🐛 Sorun Giderme

### Güncelleme Kontrolü Yapılamıyor

1. İnternet bağlantınızı kontrol edin
2. GitHub repository bilgilerinin doğru olduğundan emin olun
3. GitHub API'nin erişilebilir olduğunu kontrol edin

### Sürüm Karşılaştırması Çalışmıyor

1. Sürüm numaralarının `X.Y.Z` formatında olduğundan emin olun
2. GitHub Release'deki `tag_name` formatını kontrol edin (örn: `v1.0.1` veya `1.0.1`)

### İndirme Başarısız

1. GitHub Release'de dosya eklendiğinden emin olun
2. Dosya adının `.exe` veya `.msi` ile bittiğinden emin olun
3. Dosya boyutunun çok büyük olmadığını kontrol edin

## 📚 Ek Kaynaklar

- [GitHub Releases API Dokümantasyonu](https://docs.github.com/en/rest/releases/releases)
- [.NET Assembly Versioning](https://docs.microsoft.com/en-us/dotnet/standard/assembly/versioning)
- [AutoUpdater.NET](https://github.com/ravibpatel/AutoUpdater.NET)

Publish exe oluşturma:
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish-single

Patch oluşturma:
.\create-patch.ps1 -OldExe "v2.1.6.exe" -NewExe "v2.1.7.exe" -OutputPatch "v2.1.6-to-v2.1.7.patch"

Exe'yi v2.1.7.exe olarak kaydedin:
Copy-Item "DocumentSearch\bin\Release\net8.0-windows\win-x64\publish\DocumentSearch.exe" -Destination "v2.1.7.exe"