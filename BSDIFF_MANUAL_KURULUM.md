# 🔧 bsdiff Manuel Kurulum (Chocolatey Olmadan)

## 📥 Adım 1: bsdiff İndir

1. **GitHub'dan indir:**
   - https://github.com/mendsley/bsdiff/releases
   - En son sürümü indirin (örn: `bsdiff-4.3-win32.zip` veya `bsdiff-4.3-win64.zip`)

2. **Alternatif kaynaklar:**
   - https://github.com/mendsley/bsdiff
   - Veya: https://sourceforge.net/projects/bsdiff/

---

## 📦 Adım 2: Dosyaları Çıkar

1. İndirilen zip dosyasını açın
2. İçinden `bsdiff.exe` ve `bspatch.exe` dosyalarını bulun

---

## 📁 Adım 3: Klasöre Koy

**Seçenek 1: System32'ye koy (Tüm sistem için)**
```powershell
# PowerShell'i Yönetici olarak açın
Copy-Item "bsdiff.exe" -Destination "C:\Windows\System32\bsdiff.exe"
Copy-Item "bspatch.exe" -Destination "C:\Windows\System32\bspatch.exe"
```

**Seçenek 2: Özel klasöre koy (Önerilen)**
```powershell
# Klasör oluştur
New-Item -ItemType Directory -Path "C:\Tools\bsdiff" -Force

# Dosyaları kopyala
Copy-Item "bsdiff.exe" -Destination "C:\Tools\bsdiff\bsdiff.exe"
Copy-Item "bspatch.exe" -Destination "C:\Tools\bsdiff\bspatch.exe"
```

---

## 🔧 Adım 4: PATH'e Ekle (Seçenek 2 için)

1. **Windows Ayarlar:**
   - Windows tuşu + R → `sysdm.cpl` → Enter
   - "Gelişmiş" sekmesi → "Ortam Değişkenleri"
   - "Sistem değişkenleri" altında "Path" seçin → "Düzenle"
   - "Yeni" → `C:\Tools\bsdiff` → Tamam

2. **PowerShell ile (Yönetici olarak):**
   ```powershell
   [Environment]::SetEnvironmentVariable("Path", $env:Path + ";C:\Tools\bsdiff", [EnvironmentVariableTarget]::Machine)
   ```

3. **PowerShell'i yeniden başlatın**

---

## ✅ Adım 5: Kontrol Et

```powershell
bsdiff
```

Eğer hata alıyorsanız, PowerShell'i yeniden başlatın veya PATH'i kontrol edin.

---

## 🚀 Kullanım

Artık script'iniz bsdiff'i otomatik bulacak ve kullanacak:

```powershell
.\create-patch.ps1 -OldExe "v2.1.6.exe" -NewExe "v2.1.7.exe" -OutputPatch "v2.1.6-to-v2.1.7.patch"
```

---

## 💡 Hızlı Çözüm

Eğer PATH eklemek istemiyorsanız, script'i güncelleyebilirim ki bsdiff'in tam yolunu kullanabilsin. Söyleyin, hemen yapayım!

