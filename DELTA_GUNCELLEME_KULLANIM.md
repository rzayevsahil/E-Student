# 🔄 Delta Güncelleme Kullanım Kılavuzu

## 📋 Genel Bakış

Delta güncelleme sistemi, uygulamanın sadece değişen kısımlarını indirerek güncelleme sürecini hızlandırır ve veri kullanımını azaltır.

**Örnek:**
- Tam Exe: 85 MB
- Patch Dosyası: 2-5 MB (%95 küçültme) ✅

---

## 🚀 Release Oluşturma Süreci

### Adım 1: Yeni Sürümü Hazırla

1. `DocumentSearch.csproj` dosyasında sürüm numarasını güncelle:
   ```xml
   <Version>2.1.5</Version>
   ```

2. Projeyi build et ve publish yap:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

3. Exe dosyası şu konumda olacak:
   ```
   bin\Release\net8.0-windows\win-x64\publish\DocumentSearch.exe
   ```

### Adım 2: Patch Dosyası Oluştur

#### Yöntem 1: PowerShell Script (Önerilen)

```powershell
.\create-patch.ps1 -OldExe "v2.1.4.exe" -NewExe "v2.1.5.exe" -OutputPatch "v2.1.4-to-v2.1.5.patch"
```

**Not:** Eski exe dosyasını saklamanız gerekiyor! Her yeni release için önceki release'in exe'sine ihtiyacınız var.

#### Yöntem 2: Manuel (bsdiff kullanarak)

Eğer PowerShell script çalışmazsa, bsdiff komut satırı aracını kullanabilirsiniz:

```bash
bsdiff v2.1.4.exe v2.1.5.exe v2.1.4-to-v2.1.5.patch
```

**bsdiff İndirme:**
- Windows: https://github.com/mendsley/bsdiff/releases
- veya: `choco install bsdiff` (Chocolatey ile)

### Adım 3: GitHub Release Oluştur

1. GitHub'da yeni release oluştur:
   - Tag: `v2.1.5`
   - Title: `v2.1.5`
   - Description: Değişiklik notları

2. **İki dosya yükle:**
   - ✅ **Tam Exe:** `DocumentSearch.exe` (85 MB) - İlk kurulum için
   - ✅ **Patch:** `v2.1.4-to-v2.1.5.patch` (2-5 MB) - Güncelleme için

**Önemli:** Patch dosyasının adı şu formatta olmalı:
```
v{eski_sürüm}-to-v{yeni_sürüm}.patch
```

Örnek: `v2.1.4-to-v2.1.5.patch`

---

## 🔄 Güncelleme Süreci (Kullanıcı Tarafı)

### Senaryo 1: Delta Güncelleme (Hızlı)

```
Kullanıcı: v2.1.4
GitHub: v2.1.5
Patch: v2.1.4-to-v2.1.5.patch (2 MB)

1. Uygulama patch dosyasını kontrol eder ✅
2. Patch indiriliyor (2 MB) ✅
3. Patch uygulanıyor (1-2 saniye) ✅
4. Yeni exe oluşuyor ✅
5. Uygulama güncelleniyor ✅
```

### Senaryo 2: Patch Yok (Fallback)

```
Kullanıcı: v2.1.0 (çok eski)
GitHub: v2.1.5
Patch: Yok (çok eski sürüm)

1. Tam exe indiriliyor (85 MB) ⚠️
2. Normal güncelleme yapılıyor ✅
```

---

## 📝 Patch Dosyası Adlandırma Kuralları

### ✅ Doğru Format:
```
v2.1.4-to-v2.1.5.patch
v2.1.3-to-v2.1.4.patch
v2.0.0-to-v2.1.0.patch
```

### ❌ Yanlış Format:
```
patch-v2.1.5.patch
v2.1.5.patch
update.patch
```

---

## 🔧 Sorun Giderme

### Problem: Patch oluşturulamıyor

**Çözüm 1:** PowerShell script çalışmıyorsa:
```bash
# bsdiff komut satırı aracını kullan
bsdiff old.exe new.exe patch.patch
```

**Çözüm 2:** DeltaCompressionDotNet DLL bulunamıyor:
```bash
# NuGet paketini yükle
dotnet add package DeltaCompressionDotNet
```

### Problem: Patch uygulanamıyor

**Çözüm:** Uygulama otomatik olarak tam exe'ye geçer (fallback). Kullanıcı fark etmez.

### Problem: Patch dosyası çok büyük

**Neden:** İki sürüm arasında çok fazla değişiklik var.

**Çözüm:** Normal, büyük değişikliklerde patch dosyası da büyük olur. Yine de tam exe'den küçük olacaktır.

---

## 💡 İpuçları

1. **Eski Exe'leri Saklayın:** Her release için önceki release'in exe'sini saklayın. Patch oluşturmak için gerekli.

2. **Patch Boyutu:** Genellikle patch dosyası tam exe'nin %5-10'u kadar olur.

3. **Fallback:** Patch yoksa veya başarısız olursa, uygulama otomatik olarak tam exe'yi indirir.

4. **İlk Kurulum:** İlk kurulum için her zaman tam exe gerekir. Patch sadece güncelleme için kullanılır.

---

## 📊 Örnek Senaryo

### Release 1: v2.1.4
- GitHub Release: `v2.1.4`
- Dosyalar:
  - `DocumentSearch.exe` (85 MB) ✅

### Release 2: v2.1.5
- GitHub Release: `v2.1.5`
- Dosyalar:
  - `DocumentSearch.exe` (85 MB) ✅ (ilk kurulum için)
  - `v2.1.4-to-v2.1.5.patch` (2 MB) ✅ (güncelleme için)

### Kullanıcı Deneyimi:
- **v2.1.4 kullanıcısı:** 2 MB patch indirir ✅
- **v2.1.0 kullanıcısı:** 85 MB tam exe indirir (patch yok) ⚠️
- **Yeni kullanıcı:** 85 MB tam exe indirir ✅

---

## ✅ Kontrol Listesi

Her release için:

- [ ] Sürüm numarası güncellendi (`DocumentSearch.csproj`)
- [ ] Proje build edildi ve publish yapıldı
- [ ] Eski exe dosyası mevcut (patch oluşturmak için)
- [ ] Patch dosyası oluşturuldu (`create-patch.ps1` veya `bsdiff`)
- [ ] GitHub Release oluşturuldu
- [ ] Tam exe yüklendi (ilk kurulum için)
- [ ] Patch dosyası yüklendi (güncelleme için)
- [ ] Patch dosyası adı doğru formatta (`vX.Y.Z-to-vA.B.C.patch`)

---

## 🎯 Sonuç

Delta güncelleme sistemi sayesinde:
- ✅ **%95 daha az veri** kullanımı
- ✅ **10x daha hızlı** güncelleme
- ✅ **Daha iyi kullanıcı deneyimi**

**Not:** İlk kurulum için her zaman tam exe gerekir. Patch sadece mevcut kullanıcıların güncellemesi için kullanılır.

