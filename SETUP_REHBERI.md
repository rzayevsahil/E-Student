# Belge İçerik Arama Uygulaması - Kurulum Rehberi

## Projeyi Yayınlama (Publish)

### Yöntem 1: Self-Contained Single File (Önerilen)
Bu yöntem, .NET runtime kurulumu gerektirmez. Tek bir EXE dosyası oluşturur.

```powershell
# Proje klasörüne gidin
cd DocumentSearch

# Self-contained single file publish
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish
```

**Çıktı:** `DocumentSearch/publish/DocumentSearch.exe`

### Yöntem 2: Framework-Dependent (Daha Küçük Dosya)
Bu yöntem, hedef bilgisayarda .NET 8.0 Runtime kurulu olmalıdır.

```powershell
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

### Yöntem 3: Tüm Platformlar İçin
```powershell
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64

# Windows x86 (32-bit)
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x86

# Windows ARM64
dotnet publish -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-arm64
```

## Dağıtım

### Self-Contained Single File
1. `publish` klasöründeki `DocumentSearch.exe` dosyasını kopyalayın
2. Bu dosyayı herhangi bir Windows bilgisayarda çalıştırabilirsiniz
3. .NET runtime kurulumu gerekmez

### Framework-Dependent
1. Hedef bilgisayarda .NET 8.0 Desktop Runtime kurulu olmalıdır
2. İndirme: https://dotnet.microsoft.com/download/dotnet/8.0
3. `publish` klasöründeki tüm dosyaları kopyalayın

## Kurulum Paketi Oluşturma (İsteğe Bağlı)

### Inno Setup Kullanarak
1. Inno Setup'ı indirin: https://jrsoftware.org/isdl.php
2. Aşağıdaki script'i kullanın:

```iss
[Setup]
AppName=Belge İçerik Arama Uygulaması
AppVersion=1.0
DefaultDirName={pf}\DocumentSearch
DefaultGroupName=DocumentSearch
OutputDir=installer
OutputBaseFilename=DocumentSearchSetup
Compression=lzma
SolidCompression=yes

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Belge İçerik Arama"; Filename: "{app}\DocumentSearch.exe"
Name: "{commondesktop}\Belge İçerik Arama"; Filename: "{app}\DocumentSearch.exe"

[Run]
Filename: "{app}\DocumentSearch.exe"; Description: "Uygulamayı başlat"; Flags: nowait postinstall skipifsilent
```

## Sistem Gereksinimleri

- **İşletim Sistemi:** Windows 10/11 (64-bit veya 32-bit)
- **RAM:** Minimum 4 GB (önerilen 8 GB)
- **Disk Alanı:** 
  - Self-contained: ~150-200 MB
  - Framework-dependent: ~50 MB (+ .NET 8.0 Runtime)

## Notlar

- Self-contained single file yaklaşık 100-150 MB olabilir (tüm bağımlılıklar dahil)
- İlk çalıştırmada dosya açılması biraz zaman alabilir (sıkıştırma açılıyor)
- Uygulama verileri `%LocalAppData%\DocumentSearch` klasöründe saklanır

