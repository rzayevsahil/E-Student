# 📌 Sürüm Yönetimi - ÖNEMLİ!

## ⚠️ Kritik Kural

**Her GitHub Release oluşturduğunuzda, mutlaka `DocumentSearch.csproj` dosyasındaki `<Version>` değerini de güncellemelisiniz!**

## ❌ Yanlış Yaklaşım

```
1. csproj'da Version: 2.1.1 (eski kalıyor)
2. GitHub'da Release: v2.1.3 oluşturuluyor
3. Kullanıcı güncellemeyi indiriyor
4. ❌ Yeni exe hala 2.1.1 sürümünde
5. ❌ Kullanıcı tekrar güncelleme bildirimi alıyor (sonsuz döngü!)
```

## ✅ Doğru Yaklaşım

```
1. csproj'da Version: 2.1.1 → 2.1.3'e güncelle
2. Projeyi publish et (yeni exe 2.1.3 sürümünde olur)
3. GitHub'da Release: v2.1.3 oluştur
4. ✅ Kullanıcı güncellemeyi indiriyor
5. ✅ Yeni exe 2.1.3 sürümünde
6. ✅ Güncelleme bildirimi duruyor
```

## 📋 Doğru Sürüm Yönetimi Adımları (Otomatik GitHub Release)

### Adım 1: Kod Değişikliklerini Yap & Commit Et
```bash
git add .
git commit -m "feat: yeni özellikler ve güncellemeler"
git push origin main
```

### Adım 2: csproj'daki Sürümü Güncelle (Opsiyonel ama Önerilen)
```xml
<!-- DocumentSearch/DocumentSearch.csproj -->
<Version>1.0.0</Version>
```

### Adım 3: Git Tag Oluştur ve GitHub'a Push Et (Otomatik Release)
```bash
git tag v1.0.0
git push origin v1.0.0
```

> 🚀 **Otomatik Süreç:** Tag push yapıldığında **GitHub Actions** devreye girer:
> 1. Projeyi Windows ortamında .NET 8 ile publish eder.
> 2. `DocumentSearch.exe` ve `v1.0.0.exe` dosyalarını oluşturur.
> 3. Otomatik olarak GitHub'da `v1.0.0` isimli bir Release açıp exe dosyalarını ekler.

## 🔍 Sürüm Kontrolü Nasıl Çalışır?

### Mevcut Sürüm (Uygulamada)
```csharp
// UpdateService.cs
_currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
// Bu değer csproj'daki <Version> değerinden gelir
```

### Yeni Sürüm (GitHub'dan)
```csharp
// GitHub Releases API'den tag_name alınır
// Örnek: "v2.1.3" → "2.1.3" olarak parse edilir
```

### Karşılaştırma
```csharp
IsNewerVersion("2.1.3", "2.1.1") → true  // Güncelleme var
IsNewerVersion("2.1.3", "2.1.3") → false // Güncel
```

## 🚨 Yaygın Hatalar

### Hata 1: Sürüm Uyumsuzluğu
```
csproj: 2.1.1
GitHub Release: v2.1.3
Sonuç: ✅ Güncelleme bulunur ama kurulumdan sonra hala 2.1.1 kalır
```

### Hata 2: Tag Formatı
```
csproj: 2.1.3
GitHub Release: v2.1.3 (doğru)
GitHub Release: 2.1.3 (doğru - "v" olmadan da çalışır)
GitHub Release: Version-2.1.3 (❌ yanlış - parse edilemez)
```

### Hata 3: Sürüm Formatı
```
✅ Doğru: 1.0.0, 1.0.1, 2.1.3
❌ Yanlış: 1.0, v1.0.1, 2.1.3-beta
```

## 💡 İpuçları

1. **Sürüm Numaralandırma Stratejisi:**
   - Major.Minor.Patch (örn: 2.1.3)
   - Major: Büyük değişiklikler
   - Minor: Yeni özellikler
   - Patch: Hata düzeltmeleri

2. **Otomatik Kontrol:**
   - Her release öncesi csproj'daki sürümü kontrol edin
   - GitHub tag ile csproj version'unun eşleştiğinden emin olun

3. **Test:**
   - Release oluşturduktan sonra eski sürümle test edin
   - Güncelleme bildiriminin doğru çalıştığını doğrulayın

## 📝 Örnek Senaryo

### Senaryo: v2.1.3 Release'i

1. **Kod Değişiklikleri:**
   - Yeni özellik eklendi
   - Birkaç bug düzeltildi

2. **csproj Güncelleme:**
   ```xml
   <Version>2.1.3</Version>
   ```

3. **Publish:**
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

4. **GitHub Release:**
   - Tag: `v2.1.3`
   - Title: `Version 2.1.3 - Bug Fixes & New Features`
   - File: `DocumentSearch.exe` (2.1.3 sürümünde)

5. **Kullanıcı Deneyimi:**
   - Kullanıcı (2.1.1) uygulamayı açar
   - Sistem: "Yeni sürüm 2.1.3 mevcut!" bildirimi
   - Kullanıcı güncellemeyi indirir
   - Yeni exe kurulur (2.1.3)
   - Artık güncelleme bildirimi gösterilmez ✅

## 🔧 Sorun Giderme

### Problem: Güncelleme bulunuyor ama kurulumdan sonra hala eski sürüm

**Çözüm:** csproj'daki sürümü GitHub release tag'i ile eşleştirin.

### Problem: Güncelleme bulunmuyor

**Kontrol Listesi:**
- [ ] csproj'daki sürüm doğru mu?
- [ ] GitHub release tag'i doğru format mı? (v2.1.3 veya 2.1.3)
- [ ] GitHub repository bilgileri doğru mu? (UpdateService.cs)
- [ ] İnternet bağlantısı var mı?

### Problem: Sonsuz güncelleme döngüsü

**Sebep:** csproj sürümü güncellenmemiş, exe eski sürümde kalıyor.

**Çözüm:** csproj'daki sürümü GitHub release tag'i ile eşleştirin ve yeniden publish edin.

