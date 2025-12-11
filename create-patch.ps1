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

# Dosya kontrolü ve arama
function Find-ExeFile {
    param([string]$FileName)
    
    # Tam yol verilmişse direkt kontrol et
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
    
    # Proje dizininde ara (öncelik: DocumentSearch/DocumentSearch/publish-single)
    $searchPaths = @(
        (Join-Path (Get-Location) "DocumentSearch\DocumentSearch\publish-single"),
        (Join-Path (Get-Location) "DocumentSearch\publish-single"),
        (Join-Path (Get-Location) "publish-single"),
        (Get-Location)
    )
    
    # Önce publish-single klasörlerinde ara (DocumentSearch.exe fallback'i YOK)
    foreach ($path in $searchPaths) {
        if (Test-Path $path) {
            $fullPath = Join-Path $path $FileName
            if (Test-Path $fullPath) {
                return (Resolve-Path $fullPath).Path
            }
        }
    }
    
    # publish-single klasörlerinde bulunamadıysa, diğer klasörlerde ara (fallback)
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
            
            # DocumentSearch.exe gibi genel isimler için (sadece fallback klasörlerde)
            if ($FileName -like "v*") {
                $docSearchExe = Join-Path $path "DocumentSearch.exe"
                if (Test-Path $docSearchExe) {
                    Write-Host "⚠️  '$FileName' publish-single klasöründe bulunamadı!" -ForegroundColor Yellow
                    Write-Host "   Ancak 'DocumentSearch.exe' bulundu: $docSearchExe" -ForegroundColor Yellow
                    Write-Host "   Bu dosyayı kullanmak ister misiniz? (E/H)" -ForegroundColor Yellow
                    Write-Host "   Not: Patch oluşturmak için farklı sürümlerden exe dosyaları gerekiyor!" -ForegroundColor Cyan
                    $response = Read-Host
                    if ($response -eq "E" -or $response -eq "e" -or $response -eq "Y" -or $response -eq "y") {
                        return (Resolve-Path $docSearchExe).Path
                    }
                }
            }
        }
    }
    
    return $null
}

# Exe dosyalarını bul
$oldExePath = Find-ExeFile -FileName $OldExe
$newExePath = Find-ExeFile -FileName $NewExe

if (-not $oldExePath) {
    Write-Host "HATA: Eski exe dosyası bulunamadı: $OldExe" -ForegroundColor Red
    Write-Host ""
    Write-Host "Lütfen tam yolunu verin, örneğin:" -ForegroundColor Yellow
    Write-Host "  C:\Users\Sahil Rzayev\source\repos\DocumentSearch\bin\Release\net8.0-windows\win-x64\publish\DocumentSearch.exe" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Veya exe dosyasını script ile aynı dizine koyun." -ForegroundColor Yellow
    exit 1
}

if (-not $newExePath) {
    Write-Host "HATA: Yeni exe dosyası bulunamadı: $NewExe" -ForegroundColor Red
    Write-Host ""
    Write-Host "Lütfen tam yolunu verin, örneğin:" -ForegroundColor Yellow
    Write-Host "  C:\Users\Sahil Rzayev\source\repos\DocumentSearch\bin\Release\net8.0-windows\win-x64\publish\DocumentSearch.exe" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Veya exe dosyasını script ile aynı dizine koyun." -ForegroundColor Yellow
    exit 1
}

# Bulunan dosyaları kullan
$OldExe = $oldExePath
$NewExe = $newExePath

# Dosya boyutları
$oldSize = (Get-Item $OldExe).Length / 1MB
$newSize = (Get-Item $NewExe).Length / 1MB

# Dosyaların aynı olup olmadığını kontrol et
$oldHash = (Get-FileHash $OldExe -Algorithm MD5).Hash
$newHash = (Get-FileHash $NewExe -Algorithm MD5).Hash

if ($oldHash -eq $newHash) {
    Write-Host "⚠️  UYARI: Eski ve yeni exe dosyaları aynı!" -ForegroundColor Red
    Write-Host "   Patch oluşturmak için farklı sürümlerden exe dosyaları gerekiyor." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Eski Exe: $OldExe" -ForegroundColor Yellow
    Write-Host "   Yeni Exe: $NewExe" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Lütfen farklı sürümlerden exe dosyalarını kullanın:" -ForegroundColor Yellow
    Write-Host "   1. v2.1.6 sürümünü build edin ve exe'yi kaydedin" -ForegroundColor Cyan
    Write-Host "   2. v2.1.7 sürümünü build edin ve exe'yi kaydedin" -ForegroundColor Cyan
    Write-Host "   3. Sonra patch oluşturun" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

Write-Host "✅ Eski Exe Bulundu: $OldExe ($([math]::Round($oldSize, 2)) MB)" -ForegroundColor Green
Write-Host "✅ Yeni Exe Bulundu: $NewExe ($([math]::Round($newSize, 2)) MB)" -ForegroundColor Green
Write-Host ""

# DeltaCompressionDotNet kullanarak patch oluştur
Write-Host "Patch oluşturuluyor..." -ForegroundColor Green

try {
    # DeltaCompressionDotNet DLL'lerini otomatik bul
    $nugetPackagesPath = "$env:USERPROFILE\.nuget\packages\deltacompressiondotnet\1.0.0\lib"
    
    # Önce net48, sonra net45, sonra diğer framework'leri dene
    $frameworkPaths = @("net48", "net45", "net462", "net461", "net47", "net472")
    $deltaDllPath = $null
    $msDeltaDllPath = $null
    
    foreach ($framework in $frameworkPaths) {
        $testDeltaPath = Join-Path $nugetPackagesPath "$framework\DeltaCompressionDotNet.dll"
        $testMsDeltaPath = Join-Path $nugetPackagesPath "$framework\DeltaCompressionDotNet.MsDelta.dll"
        
        if ((Test-Path $testDeltaPath) -and (Test-Path $testMsDeltaPath)) {
            $deltaDllPath = $testDeltaPath
            $msDeltaDllPath = $testMsDeltaPath
            Write-Host "✅ DLL'ler bulundu: $framework" -ForegroundColor Green
            break
        }
    }
    
    # Eğer hala bulunamadıysa, tüm lib klasöründe ara
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
                Write-Host "✅ DLL'ler bulundu (otomatik arama)" -ForegroundColor Green
            } else {
                $deltaDllPath = $null
                $msDeltaDllPath = $null
            }
        }
    }
    
    if (-not $deltaDllPath -or -not (Test-Path $deltaDllPath)) {
        Write-Host "HATA: DeltaCompressionDotNet DLL bulunamadı." -ForegroundColor Red
        Write-Host "Lütfen NuGet paketini yükleyin: dotnet add package DeltaCompressionDotNet" -ForegroundColor Yellow
        Write-Host "Aranan konum: $nugetPackagesPath" -ForegroundColor Yellow
        exit 1
    }
    
    if (-not $msDeltaDllPath -or -not (Test-Path $msDeltaDllPath)) {
        Write-Host "HATA: DeltaCompressionDotNet.MsDelta DLL bulunamadı." -ForegroundColor Red
        Write-Host "Aranan konum: $msDeltaDllPath" -ForegroundColor Yellow
        exit 1
    }
    
    # DLL'leri yükle
    Add-Type -Path $deltaDllPath
    Add-Type -Path $msDeltaDllPath
    
    # PatchApi DLL'ini de yükle (alternatif için)
    $patchApiDllPath = $msDeltaDllPath -replace "MsDelta", "PatchApi"
    if (Test-Path $patchApiDllPath) {
        try {
            Add-Type -Path $patchApiDllPath
            $usePatchApi = $true
        }
        catch {
            $usePatchApi = $false
        }
    } else {
        $usePatchApi = $false
    }
    
    # MsDeltaCompression kullanarak patch oluştur
    $deltaCompression = New-Object DeltaCompressionDotNet.MsDelta.MsDeltaCompression
    
    # OutputPatch tam yol değilse, DocumentSearch/DocumentSearch/publish-single klasöründe oluştur
    if (-not [System.IO.Path]::IsPathRooted($OutputPatch)) {
        # Öncelik: DocumentSearch/DocumentSearch/publish-single
        $publishSinglePath = Join-Path (Get-Location) "DocumentSearch\DocumentSearch\publish-single"
        
        # Eğer yoksa, DocumentSearch/publish-single'ı kontrol et
        if (-not (Test-Path $publishSinglePath)) {
            $publishSinglePath = Join-Path (Get-Location) "DocumentSearch\publish-single"
        }
        
        # Eğer hala yoksa, kök dizindeki publish-single'ı kontrol et
        if (-not (Test-Path $publishSinglePath)) {
            $publishSinglePath = Join-Path (Get-Location) "publish-single"
        }
        
        # publish-single klasörü yoksa oluştur
        if (-not (Test-Path $publishSinglePath)) {
            New-Item -ItemType Directory -Path $publishSinglePath -Force | Out-Null
            Write-Host "✅ 'publish-single' klasörü oluşturuldu: $publishSinglePath" -ForegroundColor Green
        }
        
        $OutputPatch = Join-Path $publishSinglePath $OutputPatch
    }
    
    # Patch dosyasının tam yolunu göster
    $patchFullPath = [System.IO.Path]::GetFullPath($OutputPatch)
    
    Write-Host "Patch oluşturuluyor..." -ForegroundColor Cyan
    Write-Host "Konum: $patchFullPath" -ForegroundColor Yellow
    
    # Eğer patch dosyası zaten varsa sil
    if (Test-Path $patchFullPath) {
        Remove-Item $patchFullPath -Force
        Write-Host "⚠️  Mevcut patch dosyası silindi" -ForegroundColor Yellow
    }
    
    # CreateDelta çağrısı - hata yönetimi ile
    $patchCreated = $false
    
    # Önce MsDeltaCompression ile dene
    try {
        Write-Host "MsDeltaCompression ile patch oluşturuluyor..." -ForegroundColor Cyan
        $deltaCompression.CreateDelta($OldExe, $NewExe, $patchFullPath)
        
        # Kısa bekleme
        Start-Sleep -Milliseconds 1000
        
        if (Test-Path $patchFullPath) {
            $fileInfo = Get-Item $patchFullPath
            if ($fileInfo.Length -gt 0) {
                $patchCreated = $true
                Write-Host "✅ Patch başarıyla oluşturuldu (MsDeltaCompression)" -ForegroundColor Green
            }
        }
    }
    catch {
        Write-Host "⚠️  MsDeltaCompression hatası: $($_.Exception.Message)" -ForegroundColor Yellow
        
        # Patch dosyası oluşmuş mu kontrol et
        Start-Sleep -Milliseconds 1000
        if (Test-Path $patchFullPath) {
            $fileInfo = Get-Item $patchFullPath
            if ($fileInfo.Length -gt 0) {
                $patchCreated = $true
                Write-Host "✅ Patch dosyası oluşturuldu (exception'a rağmen)" -ForegroundColor Green
            }
        }
    }
    
    # Eğer MsDeltaCompression başarısız olduysa, PatchApiCompression dene
    if (-not $patchCreated -and $usePatchApi) {
        try {
            Write-Host "PatchApiCompression ile patch oluşturuluyor..." -ForegroundColor Cyan
            $patchApiCompression = New-Object DeltaCompressionDotNet.PatchApi.PatchApiCompression
            $patchApiCompression.CreateDelta($OldExe, $NewExe, $patchFullPath)
            
            Start-Sleep -Milliseconds 1000
            
            if (Test-Path $patchFullPath) {
                $fileInfo = Get-Item $patchFullPath
                if ($fileInfo.Length -gt 0) {
                    $patchCreated = $true
                    Write-Host "✅ Patch başarıyla oluşturuldu (PatchApiCompression)" -ForegroundColor Green
                }
            }
        }
        catch {
            Write-Host "⚠️  PatchApiCompression hatası: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
    
    # Hala oluşmadıysa hata ver
    if (-not $patchCreated) {
        Write-Host "HATA: Patch oluşturulamadı!" -ForegroundColor Red
        Write-Host "Lütfen bsdiff komut satırı aracını kullanın:" -ForegroundColor Yellow
        Write-Host "  bsdiff `"$OldExe`" `"$NewExe`" `"$patchFullPath`"" -ForegroundColor Cyan
        throw "Patch oluşturulamadı"
    }
    
    # Kısa bir bekleme (dosya yazma işleminin tamamlanması için)
    if ($patchCreated) {
        Start-Sleep -Milliseconds 1000
    }
    
    # Patch boyutu
    if (Test-Path $patchFullPath) {
        $patchSize = (Get-Item $patchFullPath).Length / 1MB
        $savings = (1 - ($patchSize / $newSize)) * 100
        
        Write-Host ""
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
        Write-Host "=== BAŞARILI ===" -ForegroundColor Green
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
        Write-Host ""
        Write-Host "📍 PATCH DOSYASI KONUMU:" -ForegroundColor Yellow -BackgroundColor DarkBlue
        Write-Host "   $patchFullPath" -ForegroundColor White -BackgroundColor DarkBlue
        Write-Host ""
        Write-Host "Patch Boyutu: $([math]::Round($patchSize, 2)) MB" -ForegroundColor Green
        Write-Host "Tasarruf: %$([math]::Round($savings, 1))" -ForegroundColor Green
        Write-Host ""
        Write-Host "Bu patch dosyasını GitHub Release'e yükleyin!" -ForegroundColor Cyan
        Write-Host ""
        
        # Windows Explorer'da patch dosyasını göster
        try {
            Start-Process "explorer.exe" -ArgumentList "/select,`"$patchFullPath`""
            Write-Host "✅ Windows Explorer'da patch dosyası açıldı!" -ForegroundColor Green
        }
        catch {
            Write-Host "⚠️  Windows Explorer açılamadı" -ForegroundColor Yellow
        }
        
        Write-Host ""
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
        Write-Host "Çıkmak için bir tuşa basın..." -ForegroundColor Yellow
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
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

