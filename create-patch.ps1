# Delta Güncelleme Patch Oluşturma Script'i
# Kullanım: .\create-patch.ps1 -OldExe "v2.1.4.exe" -NewExe "v2.1.5.exe" -OutputPatch "v2.1.4-to-v2.1.5.patch"

param(
    [Parameter(Mandatory=$true)]
    [string]$OldExe,
    
    [Parameter(Mandatory=$true)]
    [string]$NewExe,
    
    [Parameter(Mandatory=$true)]
    [string]$OutputPatch
)

Write-Host "=== Delta Güncelleme Patch Oluşturma ===" -ForegroundColor Cyan
Write-Host ""

# Dosya kontrolü
if (-not (Test-Path $OldExe)) {
    Write-Host "HATA: Eski exe dosyası bulunamadı: $OldExe" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $NewExe)) {
    Write-Host "HATA: Yeni exe dosyası bulunamadı: $NewExe" -ForegroundColor Red
    exit 1
}

# Dosya boyutları
$oldSize = (Get-Item $OldExe).Length / 1MB
$newSize = (Get-Item $NewExe).Length / 1MB

Write-Host "Eski Exe: $OldExe ($([math]::Round($oldSize, 2)) MB)" -ForegroundColor Yellow
Write-Host "Yeni Exe: $NewExe ($([math]::Round($newSize, 2)) MB)" -ForegroundColor Yellow
Write-Host ""

# DeltaCompressionDotNet kullanarak patch oluştur
Write-Host "Patch oluşturuluyor..." -ForegroundColor Green

try {
    # DeltaCompressionDotNet DLL'lerini yükle
    $deltaDllPath = "$env:USERPROFILE\.nuget\packages\deltacompressiondotnet\1.0.0\lib\net48\DeltaCompressionDotNet.dll"
    $msDeltaDllPath = "$env:USERPROFILE\.nuget\packages\deltacompressiondotnet\1.0.0\lib\net48\DeltaCompressionDotNet.MsDelta.dll"
    
    if (-not (Test-Path $deltaDllPath)) {
        Write-Host "HATA: DeltaCompressionDotNet DLL bulunamadı. NuGet paketini yükleyin: dotnet add package DeltaCompressionDotNet" -ForegroundColor Red
        exit 1
    }
    
    # DLL'leri yükle
    Add-Type -Path $deltaDllPath
    Add-Type -Path $msDeltaDllPath
    
    # MsDeltaCompression kullanarak patch oluştur
    $deltaCompression = New-Object DeltaCompressionDotNet.MsDelta.MsDeltaCompression
    
    Write-Host "Patch oluşturuluyor: $OutputPatch" -ForegroundColor Cyan
    $deltaCompression.CreateDelta($OldExe, $NewExe, $OutputPatch)
    
    # Patch boyutu
    if (Test-Path $OutputPatch) {
        $patchSize = (Get-Item $OutputPatch).Length / 1MB
        $savings = (1 - ($patchSize / $newSize)) * 100
        
        Write-Host ""
        Write-Host "=== BAŞARILI ===" -ForegroundColor Green
        Write-Host "Patch Dosyası: $OutputPatch" -ForegroundColor Green
        Write-Host "Patch Boyutu: $([math]::Round($patchSize, 2)) MB" -ForegroundColor Green
        Write-Host "Tasarruf: %$([math]::Round($savings, 1))" -ForegroundColor Green
        Write-Host ""
        Write-Host "Bu patch dosyasını GitHub Release'e yükleyin!" -ForegroundColor Cyan
    } else {
        Write-Host "HATA: Patch dosyası oluşturulamadı!" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "HATA: Patch oluşturma sırasında hata: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternatif: bsdiff komut satırı aracını kullanabilirsiniz:" -ForegroundColor Yellow
    Write-Host "  bsdiff $OldExe $NewExe $OutputPatch" -ForegroundColor Yellow
    exit 1
}

