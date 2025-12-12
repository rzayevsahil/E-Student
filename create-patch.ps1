# Delta Guncelleme Patch Olusturma Script'i (DeltaCompressionDotNet kullanarak)
# Kullanim: .\create-patch.ps1 -OldExe "v2.1.4.exe" -NewExe "v2.1.5.exe" -OutputPatch "v2.1.4-to-v2.1.5.patch"

param(
    [Parameter(Mandatory=$true)]
    [string]$OldExe,
    
    [Parameter(Mandatory=$true)]
    [string]$NewExe,
    
    [Parameter(Mandatory=$true)]
    [string]$OutputPatch
)

Write-Host "=== Delta Guncelleme Patch Olusturma (DeltaCompressionDotNet) ===" -ForegroundColor Cyan
Write-Host ""

# Dosya kontrolu ve arama
function Find-ExeFile {
    param([string]$FileName)
    
    # Tam yol verilmisse direkt kontrol et
    if ([System.IO.Path]::IsPathRooted($FileName)) {
        if (Test-Path $FileName) {
            return $FileName
        }
        return $null
    }
    
    # Mevcut dizinde ara
    if (Test-Path $FileName) {
        return (Resolve-Path $FileName).Path
    }
    
    # Proje dizininde ara (oncelik: DocumentSearch/DocumentSearch/publish-single)
    $searchPaths = @(
        (Join-Path (Get-Location) "DocumentSearch\DocumentSearch\publish-single"),
        (Join-Path (Get-Location) "DocumentSearch\publish-single"),
        (Join-Path (Get-Location) "publish-single"),
        (Get-Location)
    )
    
    # Once publish-single klasorlerinde ara
    foreach ($path in $searchPaths) {
        if (Test-Path $path) {
            $fullPath = Join-Path $path $FileName
            if (Test-Path $fullPath) {
                return (Resolve-Path $fullPath).Path
            }
        }
    }
    
    # publish-single klasorlerinde bulunamadiysa, diger klasorlerde ara (fallback)
    $fallbackPaths = @(
        (Join-Path (Get-Location) "bin\Release\net8.0-windows\win-x64\publish"),
        (Join-Path (Get-Location) "DocumentSearch\bin\Release\net8.0-windows\win-x64\publish")
    )
    
    foreach ($path in $fallbackPaths) {
        if (Test-Path $path) {
            $fullPath = Join-Path $path $FileName
            if (Test-Path $fullPath) {
                return (Resolve-Path $fullPath).Path
            }
        }
    }
    
    return $null
}

# Exe dosyalarini bul
$oldExePath = Find-ExeFile -FileName $OldExe
$newExePath = Find-ExeFile -FileName $NewExe

if (-not $oldExePath) {
    Write-Host "HATA: Eski exe dosyasi bulunamadi: $OldExe" -ForegroundColor Red
    Write-Host ""
    Write-Host "Lutfen tam yolunu verin, ornegin:" -ForegroundColor Yellow
    Write-Host "  C:\Users\Sahil Rzayev\source\repos\DocumentSearch\DocumentSearch\publish-single\v2.1.6.exe" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Veya exe dosyasini script ile ayni dizine koyun." -ForegroundColor Yellow
    exit 1
}

if (-not $newExePath) {
    Write-Host "HATA: Yeni exe dosyasi bulunamadi: $NewExe" -ForegroundColor Red
    Write-Host ""
    Write-Host "Lutfen tam yolunu verin, ornegin:" -ForegroundColor Yellow
    Write-Host "  C:\Users\Sahil Rzayev\source\repos\DocumentSearch\DocumentSearch\publish-single\v2.1.7.exe" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Veya exe dosyasini script ile ayni dizine koyun." -ForegroundColor Yellow
    exit 1
}

# Bulunan dosyalari kullan
$OldExe = $oldExePath
$NewExe = $newExePath

# Dosya boyutlari
$oldSize = (Get-Item $OldExe).Length / 1MB
$newSize = (Get-Item $NewExe).Length / 1MB

# Dosyalarin ayni olup olmadigini kontrol et
$oldHash = (Get-FileHash $OldExe -Algorithm MD5).Hash
$newHash = (Get-FileHash $NewExe -Algorithm MD5).Hash

if ($oldHash -eq $newHash) {
    Write-Host "UYARI: Eski ve yeni exe dosyalari ayni!" -ForegroundColor Red
    Write-Host "   Patch olusturmak icin farkli surumlerden exe dosyalari gerekiyor." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Eski Exe: $OldExe" -ForegroundColor Yellow
    Write-Host "   Yeni Exe: $NewExe" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host "Eski Exe Bulundu: $OldExe ($([math]::Round($oldSize, 2)) MB)" -ForegroundColor Green
Write-Host "Yeni Exe Bulundu: $NewExe ($([math]::Round($newSize, 2)) MB)" -ForegroundColor Green
Write-Host ""

# DeltaCompressionDotNet DLL'lerini bul ve yukle
Write-Host "DeltaCompressionDotNet kutuphanesi araniyor..." -ForegroundColor Cyan

try {
    # NuGet paket klasorunu bul
    $nugetPackagesPath = "$env:USERPROFILE\.nuget\packages\deltacompressiondotnet\1.0.0\lib"
    
    # Once net48, sonra net45, sonra diger framework'leri dene
    $frameworkPaths = @("net48", "net45", "net462", "net461", "net47", "net472")
    $deltaDllPath = $null
    $msDeltaDllPath = $null
    
    foreach ($framework in $frameworkPaths) {
        $testDeltaPath = Join-Path $nugetPackagesPath "$framework\DeltaCompressionDotNet.dll"
        $testMsDeltaPath = Join-Path $nugetPackagesPath "$framework\DeltaCompressionDotNet.MsDelta.dll"
        
        if ((Test-Path $testDeltaPath) -and (Test-Path $testMsDeltaPath)) {
            $deltaDllPath = $testDeltaPath
            $msDeltaDllPath = $testMsDeltaPath
            Write-Host "DLL'ler bulundu: $framework" -ForegroundColor Green
            break
        }
    }
    
    # Eger hala bulunamadiysa, tum lib klasorunde ara
    if (-not $deltaDllPath) {
        $allDlls = Get-ChildItem -Path $nugetPackagesPath -Recurse -Filter "DeltaCompressionDotNet.dll" -ErrorAction SilentlyContinue
        if ($allDlls.Count -gt 0) {
            $deltaDllPath = $allDlls[0].FullName
            $msDeltaDllPath = $deltaDllPath -replace "DeltaCompressionDotNet\.dll$", "DeltaCompressionDotNet.MsDelta.dll"
            
            if (-not (Test-Path $msDeltaDllPath)) {
                $msDeltaDlls = Get-ChildItem -Path $nugetPackagesPath -Recurse -Filter "DeltaCompressionDotNet.MsDelta.dll" -ErrorAction SilentlyContinue
                if ($msDeltaDlls.Count -gt 0) {
                    $msDeltaDllPath = $msDeltaDlls[0].FullName
                }
            }
            
            if ($deltaDllPath -and $msDeltaDllPath -and (Test-Path $deltaDllPath) -and (Test-Path $msDeltaDllPath)) {
                Write-Host "DLL'ler bulundu (otomatik arama)" -ForegroundColor Green
            } else {
                $deltaDllPath = $null
                $msDeltaDllPath = $null
            }
        }
    }
    
    if (-not $deltaDllPath -or -not (Test-Path $deltaDllPath)) {
        Write-Host "HATA: DeltaCompressionDotNet DLL bulunamadi." -ForegroundColor Red
        Write-Host "Lutfen NuGet paketini yukleyin: dotnet add package DeltaCompressionDotNet" -ForegroundColor Yellow
        Write-Host "Aranan konum: $nugetPackagesPath" -ForegroundColor Yellow
        exit 1
    }
    
    if (-not $msDeltaDllPath -or -not (Test-Path $msDeltaDllPath)) {
        Write-Host "HATA: DeltaCompressionDotNet.MsDelta DLL bulunamadi." -ForegroundColor Red
        Write-Host "Aranan konum: $msDeltaDllPath" -ForegroundColor Yellow
        exit 1
    }
    
    # DLL'leri yukle
    Add-Type -Path $deltaDllPath
    Add-Type -Path $msDeltaDllPath
    Write-Host "DeltaCompressionDotNet kutuphanesi yuklendi" -ForegroundColor Green
    
    # Patch dosyasi her zaman publish-single klasorune olusturulacak
    # Oncelik: DocumentSearch/DocumentSearch/publish-single
    $publishSinglePath = Join-Path (Get-Location) "DocumentSearch\DocumentSearch\publish-single"
    
    # Eger yoksa, DocumentSearch/publish-single'i kontrol et
    if (-not (Test-Path $publishSinglePath)) {
        $publishSinglePath = Join-Path (Get-Location) "DocumentSearch\publish-single"
    }
    
    # Eger hala yoksa, kok dizindeki publish-single'i kontrol et
    if (-not (Test-Path $publishSinglePath)) {
        $publishSinglePath = Join-Path (Get-Location) "publish-single"
    }
    
    # publish-single klasoru yoksa olustur
    if (-not (Test-Path $publishSinglePath)) {
        New-Item -ItemType Directory -Path $publishSinglePath -Force | Out-Null
        Write-Host "'publish-single' klasoru olusturuldu: $publishSinglePath" -ForegroundColor Green
    }
    
    # OutputPatch'ten sadece dosya adini al (tam yol verilmisse de)
    $patchFileName = [System.IO.Path]::GetFileName($OutputPatch)
    
    # Her zaman publish-single klasorune olustur
    $OutputPatch = Join-Path $publishSinglePath $patchFileName
    
    # Patch dosyasinin tam yolunu goster
    $patchFullPath = [System.IO.Path]::GetFullPath($OutputPatch)
    
    Write-Host ""
    Write-Host "Patch olusturuluyor..." -ForegroundColor Cyan
    Write-Host "Konum: $patchFullPath" -ForegroundColor Yellow
    Write-Host ""
    
    # Eger patch dosyasi zaten varsa sil
    if (Test-Path $patchFullPath) {
        Remove-Item $patchFullPath -Force
        Write-Host "Mevcut patch dosyasi silindi" -ForegroundColor Yellow
    }
    
    # PatchApi DLL'ini de yukle (alternatif icin)
    $patchApiDllPath = $msDeltaDllPath -replace "MsDelta", "PatchApi"
    $usePatchApi = $false
    
    if (Test-Path $patchApiDllPath) {
        try {
            Add-Type -Path $patchApiDllPath
            $usePatchApi = $true
            Write-Host "PatchApiCompression yuklendi (alternatif)" -ForegroundColor Green
        }
        catch {
            $usePatchApi = $false
        }
    }
    
    # MsDeltaCompression kullanarak patch olustur
    $deltaCompression = New-Object DeltaCompressionDotNet.MsDelta.MsDeltaCompression
    
    Write-Host "MsDeltaCompression ile patch olusturuluyor..." -ForegroundColor Cyan
    
    $patchCreated = $false
    
    try {
        $deltaCompression.CreateDelta($OldExe, $NewExe, $patchFullPath)
        $patchCreated = $true
    }
    catch {
        # MsDeltaCompression bazen basarili olsa bile exception firlatir
        Write-Host "MsDeltaCompression mesaji: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    # Patch dosyasinin olusup olusmadigini kontrol et
    Start-Sleep -Milliseconds 2000
    
    if (Test-Path $patchFullPath) {
        $fileInfo = Get-Item $patchFullPath
        if ($fileInfo.Length -gt 0) {
            $patchCreated = $true
            Write-Host "Patch basariyla olusturuldu (MsDeltaCompression)!" -ForegroundColor Green
        }
    }
    
    # Eger MsDeltaCompression basarisiz olduysa, PatchApiCompression dene
    if (-not $patchCreated -and $usePatchApi) {
        Write-Host "PatchApiCompression ile patch olusturuluyor..." -ForegroundColor Cyan
        
        try {
            $patchApiCompression = New-Object DeltaCompressionDotNet.PatchApi.PatchApiCompression
            $patchApiCompression.CreateDelta($OldExe, $NewExe, $patchFullPath)
            
            Start-Sleep -Milliseconds 2000
            
            if (Test-Path $patchFullPath) {
                $fileInfo = Get-Item $patchFullPath
                if ($fileInfo.Length -gt 0) {
                    $patchCreated = $true
                    Write-Host "Patch basariyla olusturuldu (PatchApiCompression)!" -ForegroundColor Green
                }
            }
        }
        catch {
            Write-Host "PatchApiCompression hatasi: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
    
    if (-not $patchCreated) {
        Write-Host "HATA: Patch dosyasi olusturulamadi!" -ForegroundColor Red
        Write-Host "   Hem MsDeltaCompression hem de PatchApiCompression basarisiz oldu." -ForegroundColor Yellow
        Write-Host "   Alternatif: Tam exe indirmeyi kullanin." -ForegroundColor Cyan
        exit 1
    }
    
    # Patch boyutu ve istatistikler
    $patchFileInfo = Get-Item $patchFullPath
    $patchSize = $patchFileInfo.Length / 1MB
    $savings = (1 - ($patchSize / $newSize)) * 100
    
    Write-Host ""
    Write-Host "===============================================================" -ForegroundColor Green
    Write-Host "=== BASARILI ===" -ForegroundColor Green
    Write-Host "===============================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "PATCH DOSYASI KONUMU:" -ForegroundColor Yellow -BackgroundColor DarkBlue
    Write-Host "   $patchFullPath" -ForegroundColor White -BackgroundColor DarkBlue
    Write-Host ""
    Write-Host "Patch Boyutu: $([math]::Round($patchSize, 2)) MB" -ForegroundColor Green
    Write-Host "Yeni Exe Boyutu: $([math]::Round($newSize, 2)) MB" -ForegroundColor Cyan
    Write-Host "Tasarruf: %$([math]::Round($savings, 1))" -ForegroundColor Green
    Write-Host ""
    
    if ($savings -lt 10) {
        Write-Host "UYARI: Patch dosyasi cok buyuk (tasarruf %10'dan az)" -ForegroundColor Yellow
        Write-Host "   Bu durumda tam exe indirmek daha mantikli olabilir." -ForegroundColor Cyan
    }
    else {
        Write-Host "Patch dosyasi boyutu uygun!" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "Bu patch dosyasini GitHub Release'e yukleyin!" -ForegroundColor Cyan
    Write-Host ""
    
    # Windows Explorer'da patch dosyasini goster
    try {
        Start-Process "explorer.exe" -ArgumentList "/select,`"$patchFullPath`""
        Write-Host "Windows Explorer'da patch dosyasi acildi!" -ForegroundColor Green
    }
    catch {
        Write-Host "Windows Explorer acilamadi" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "===============================================================" -ForegroundColor Green
    Write-Host "Cikmak icin bir tusa basin..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}
catch {
    Write-Host "HATA: Patch olusturma sirasinda hata: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Hata Detayi: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.InnerException) {
        Write-Host "Ic Hata: $($_.Exception.InnerException.Message)" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Lutfen sunlari kontrol edin:" -ForegroundColor Yellow
    Write-Host "  1. DeltaCompressionDotNet NuGet paketi yuklu mu?" -ForegroundColor Cyan
    Write-Host "  2. Exe dosyalari farkli surumlerden mi?" -ForegroundColor Cyan
    Write-Host "  3. Dosya yollari dogru mu?" -ForegroundColor Cyan
    exit 1
}
