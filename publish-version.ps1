# Versiyonlu Publish Script
# Kullanim: .\publish-version.ps1 -Version "v2.2.0"
# Veya: .\publish-version.ps1 "v2.2.0"

param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

Write-Host "=== Versiyonlu Publish Islemi ===" -ForegroundColor Cyan
Write-Host ""

# Versiyon formatini kontrol et (v ile baslamiyorsa ekle)
if (-not $Version.StartsWith("v", [System.StringComparison]::OrdinalIgnoreCase)) {
    $Version = "v$Version"
}

Write-Host "Versiyon: $Version" -ForegroundColor Yellow
Write-Host ""

# Publish dizini
$publishDir = "./publish-single"
$exeName = $Version

Write-Host "Publish islemi basliyor..." -ForegroundColor Cyan
Write-Host ""

# Publish komutu - AssemblyName ile exe ismini belirle
dotnet publish -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:AssemblyName=$exeName `
    -o $publishDir

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Publish basarili!" -ForegroundColor Green
    Write-Host ""
    
    # Olusan exe dosyasini kontrol et
    $exePath = Join-Path $publishDir "$exeName.exe"
    if (Test-Path $exePath) {
        $fileInfo = Get-Item $exePath
        $fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
        Write-Host "Olusan dosya: $exePath" -ForegroundColor Green
        Write-Host "Dosya boyutu: $fileSizeMB MB" -ForegroundColor Green
        Write-Host ""
        
        # Dosyayi acmak isteyip istemedigini sor
        $open = Read-Host "Dosya konumunu acmak ister misiniz? (E/H)"
        if ($open -eq "E" -or $open -eq "e") {
            explorer.exe (Split-Path -Parent $exePath)
        }
    } else {
        Write-Host "UYARI: Exe dosyasi bulunamadi: $exePath" -ForegroundColor Yellow
    }
} else {
    Write-Host ""
    Write-Host "Publish basarisiz!" -ForegroundColor Red
    exit 1
}

