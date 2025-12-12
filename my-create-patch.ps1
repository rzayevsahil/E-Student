# Kendi Patch Sistemi - Binary Diff/Patch Olusturma
# Kullanim: .\my-create-patch.ps1 -OldExe "v1.0.0.exe" -NewExe "v2.1.7.exe" -OutputPatch "v1.0.0-to-v2.1.7.patch"

param(
    [Parameter(Mandatory=$true)]
    [string]$OldExe,
    
    [Parameter(Mandatory=$true)]
    [string]$NewExe,
    
    [Parameter(Mandatory=$true)]
    [string]$OutputPatch
)

Write-Host "=== Kendi Patch Sistemi - Binary Diff Olusturma ===" -ForegroundColor Cyan
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
    
    # Proje dizininde ara
    $searchPaths = @(
        (Join-Path (Get-Location) "DocumentSearch\DocumentSearch\publish-single"),
        (Join-Path (Get-Location) "DocumentSearch\publish-single"),
        (Join-Path (Get-Location) "publish-single"),
        (Get-Location)
    )
    
    foreach ($path in $searchPaths) {
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
    exit 1
}

if (-not $newExePath) {
    Write-Host "HATA: Yeni exe dosyasi bulunamadi: $NewExe" -ForegroundColor Red
    exit 1
}

$OldExe = $oldExePath
$NewExe = $newExePath

# Dosya boyutlari
$oldSize = (Get-Item $OldExe).Length / 1MB
$newSize = (Get-Item $NewExe).Length / 1MB

Write-Host "Eski Exe: $OldExe ($([math]::Round($oldSize, 2)) MB)" -ForegroundColor Green
Write-Host "Yeni Exe: $NewExe ($([math]::Round($newSize, 2)) MB)" -ForegroundColor Green
Write-Host ""

# Patch dosyasi konumu (publish-single klasorune)
$publishSinglePath = Join-Path (Get-Location) "DocumentSearch\DocumentSearch\publish-single"

if (-not (Test-Path $publishSinglePath)) {
    $publishSinglePath = Join-Path (Get-Location) "DocumentSearch\publish-single"
}

if (-not (Test-Path $publishSinglePath)) {
    $publishSinglePath = Join-Path (Get-Location) "publish-single"
}

if (-not (Test-Path $publishSinglePath)) {
    New-Item -ItemType Directory -Path $publishSinglePath -Force | Out-Null
}

$patchFileName = [System.IO.Path]::GetFileName($OutputPatch)
$patchFullPath = Join-Path $publishSinglePath $patchFileName

Write-Host "Patch olusturuluyor..." -ForegroundColor Cyan
Write-Host "Konum: $patchFullPath" -ForegroundColor Yellow
Write-Host ""

# Eski ve yeni dosyalari oku
Write-Host "Dosyalar okunuyor..." -ForegroundColor Cyan
$oldBytes = [System.IO.File]::ReadAllBytes($OldExe)
$newBytes = [System.IO.File]::ReadAllBytes($NewExe)

Write-Host "   Eski exe: $($oldBytes.Length) bytes" -ForegroundColor Gray
Write-Host "   Yeni exe: $($newBytes.Length) bytes" -ForegroundColor Gray
Write-Host ""

# Hash-based block comparison algoritmasi (cok daha hizli)
Write-Host "Farklar hesaplaniyor (Hash-based block comparison)..." -ForegroundColor Cyan
Write-Host ""

$changes = New-Object System.Collections.ArrayList
$oldLen = $oldBytes.Length
$newLen = $newBytes.Length
$blockSize = 4096  # 4KB bloklar (hash hesaplama icin ideal)
$minMatchLength = 256  # En az 256 byte eslesme gerekli

Write-Host "   Toplam islenecek: $([math]::Max($oldLen, $newLen)) bytes" -ForegroundColor Gray
Write-Host "   Blok boyutu: $blockSize bytes" -ForegroundColor Gray
Write-Host ""

# Hash hesaplama fonksiyonu (hizli)
function Get-BlockHash {
    param([byte[]]$data, [int]$offset, [int]$length)
    
    # Basit ve hizli hash (CRC32 benzeri)
    $hash = 0
    $end = [math]::Min($offset + $length, $data.Length)
    for ($i = $offset; $i -lt $end; $i++) {
        $hash = (($hash -shl 1) -bxor $data[$i]) -band 0xFFFFFFFF
    }
    return $hash
}

# Once hash'leri hesapla (hizli tarama)
Write-Host "   Hash'ler hesaplaniyor..." -ForegroundColor Cyan
$oldHashes = @()
$newHashes = @()

$oldBlockCount = [math]::Ceiling($oldLen / $blockSize)
$newBlockCount = [math]::Ceiling($newLen / $blockSize)
$totalBlocks = $oldBlockCount + $newBlockCount
$processedBlocks = 0

# Eski dosya hash'leri
for ($i = 0; $i -lt $oldBlockCount; $i++) {
    $offset = $i * $blockSize
    $length = [math]::Min($blockSize, $oldLen - $offset)
    $hash = Get-BlockHash -data $oldBytes -offset $offset -length $length
    $oldHashes += $hash
    $processedBlocks++
    
    # Progress goster
    $progress = ($processedBlocks / $totalBlocks) * 100
    $progressLine = "   Hash hesaplama: %$([math]::Round($progress, 1)) ($processedBlocks / $totalBlocks blok)        "
    Write-Host "`r$progressLine" -NoNewline
}

# Yeni dosya hash'leri
for ($i = 0; $i -lt $newBlockCount; $i++) {
    $offset = $i * $blockSize
    $length = [math]::Min($blockSize, $newLen - $offset)
    $hash = Get-BlockHash -data $newBytes -offset $offset -length $length
    $newHashes += $hash
    $processedBlocks++
    
    # Progress goster
    $progress = ($processedBlocks / $totalBlocks) * 100
    $progressLine = "   Hash hesaplama: %$([math]::Round($progress, 1)) ($processedBlocks / $totalBlocks blok)        "
    Write-Host "`r$progressLine" -NoNewline
}

# Son satiri temizle
Write-Host ""
Write-Host "   Hash hesaplama tamamlandi!" -ForegroundColor Green
Write-Host ""

# Hash'leri karsilastir ve farklari bul
$oldPos = 0
$newPos = 0
$oldBlockIndex = 0
$newBlockIndex = 0
$lastProgressPercent = -1
$lastProgressTime = Get-Date
$totalBytes = [math]::Max($oldLen, $newLen)
$changeStartOld = -1
$changeStartNew = -1

Write-Host "   Bloklar karsilastiriliyor..." -ForegroundColor Cyan
Write-Host ""

while ($oldBlockIndex -lt $oldBlockCount -or $newBlockIndex -lt $newBlockCount) {
    # Progress goster
    $currentProgress = [math]::Min(100, ([math]::Max($oldBlockIndex, $newBlockIndex) / [math]::Max($oldBlockCount, $newBlockCount)) * 100)
    $currentTime = Get-Date
    $timeDiff = ($currentTime - $lastProgressTime).TotalSeconds
    
    if (($currentProgress -ge $lastProgressPercent + 1) -or ($timeDiff -ge 1)) {
        $elapsed = ($currentTime - $lastProgressTime).TotalSeconds
        $processedBlocks = [math]::Max($oldBlockIndex, $newBlockIndex)
        $blocksPerSec = if ($elapsed -gt 0) { $processedBlocks / $elapsed } else { 0 }
        $remainingBlocks = [math]::Max($oldBlockCount, $newBlockCount) - $processedBlocks
        $estimatedSeconds = if ($blocksPerSec -gt 0) { $remainingBlocks / $blocksPerSec } else { 0 }
        
        $processedBytes = $processedBlocks * $blockSize
        
        # Ayni satirda guncelle (carriage return ile)
        $progressLine = "   Ilerleme: %$([math]::Round($currentProgress, 1)) | Islenen: $([math]::Round($processedBytes / 1MB, 2)) MB / $([math]::Round($totalBytes / 1MB, 2)) MB | Hiz: $([math]::Round($blocksPerSec, 1)) blok/s"
        if ($estimatedSeconds -gt 0 -and $estimatedSeconds -lt 3600) {
            $estimatedMinutes = [math]::Round($estimatedSeconds / 60, 1)
            $progressLine += " | Kalan: $estimatedMinutes dakika"
        }
        $progressLine += "        "  # Eski satiri tamamen silmek icin bosluk
        
        Write-Host "`r$progressLine" -NoNewline
        
        $lastProgressPercent = [math]::Floor($currentProgress)
        $lastProgressTime = $currentTime
    }
    
    # Hash'leri karsilastir
    if ($oldBlockIndex -lt $oldBlockCount -and $newBlockIndex -lt $newBlockCount -and 
        $oldHashes[$oldBlockIndex] -eq $newHashes[$newBlockIndex]) {
        # Hash'ler ayni - blok ayni, atla
        if ($changeStartOld -ge 0) {
            # Onceki degisikligi kaydet
            $oldChangeLength = $oldPos - $changeStartOld
            $newChangeLength = $newPos - $changeStartNew
            $newData = $newBytes[$changeStartNew..($newPos - 1)]
            
            $change = @{
                Offset = $changeStartOld
                OldLength = $oldChangeLength
                NewLength = $newChangeLength
                NewData = $newData
            }
            [void]$changes.Add($change)
            $changeStartOld = -1
            $changeStartNew = -1
        }
        
        $oldPos += $blockSize
        $newPos += $blockSize
        $oldBlockIndex++
        $newBlockIndex++
    }
    else {
        # Hash'ler farkli - bu blokta byte-byte kontrol et
        $oldBlockStart = $oldPos
        $newBlockStart = $newPos
        $oldBlockEnd = [math]::Min($oldPos + $blockSize, $oldLen)
        $newBlockEnd = [math]::Min($newPos + $blockSize, $newLen)
        
        # Degisiklik basladi
        if ($changeStartOld -lt 0) {
            $changeStartOld = $oldBlockStart
            $changeStartNew = $newBlockStart
        }
        
        # Bu blokta byte-byte karsilastir (sadece farkli bloklarda)
        $blockOldPos = $oldBlockStart
        $blockNewPos = $newBlockStart
        
        while ($blockOldPos -lt $oldBlockEnd -or $blockNewPos -lt $newBlockEnd) {
            # Ayni byte'lari bul
            $sameCount = 0
            while ($blockOldPos + $sameCount -lt $oldBlockEnd -and 
                   $blockNewPos + $sameCount -lt $newBlockEnd -and 
                   $oldBytes[$blockOldPos + $sameCount] -eq $newBytes[$blockNewPos + $sameCount]) {
                $sameCount++
            }
            
            if ($sameCount -ge $minMatchLength) {
                # Yeterince ayni byte varsa, degisikligi kaydet
                if ($blockOldPos -gt $changeStartOld -or $blockNewPos -gt $changeStartNew) {
                    $oldChangeLength = $blockOldPos - $changeStartOld
                    $newChangeLength = $blockNewPos - $changeStartNew
                    $newData = $newBytes[$changeStartNew..($blockNewPos - 1)]
                    
                    $change = @{
                        Offset = $changeStartOld
                        OldLength = $oldChangeLength
                        NewLength = $newChangeLength
                        NewData = $newData
                    }
                    [void]$changes.Add($change)
                }
                $changeStartOld = -1
                $changeStartNew = -1
                
                $blockOldPos += $sameCount
                $blockNewPos += $sameCount
            }
            else {
                # Farkli byte
                if ($changeStartOld -lt 0) {
                    $changeStartOld = $blockOldPos
                    $changeStartNew = $blockNewPos
                }
                if ($blockOldPos -lt $oldBlockEnd) { $blockOldPos++ }
                if ($blockNewPos -lt $newBlockEnd) { $blockNewPos++ }
            }
        }
        
        $oldPos = $oldBlockEnd
        $newPos = $newBlockEnd
        $oldBlockIndex++
        $newBlockIndex++
    }
}

# Son degisikligi kaydet (varsa)
if ($changeStartOld -ge 0) {
    $oldChangeLength = $oldPos - $changeStartOld
    $newChangeLength = $newPos - $changeStartNew
    $newData = $newBytes[$changeStartNew..($newPos - 1)]
    
    $change = @{
        Offset = $changeStartOld
        OldLength = $oldChangeLength
        NewLength = $newChangeLength
        NewData = $newData
    }
    [void]$changes.Add($change)
}

Write-Host ""
Write-Host "   Fark hesaplama tamamlandi!" -ForegroundColor Green

Write-Host "   Toplam degisiklik: $($changes.Count) adet" -ForegroundColor Green
Write-Host ""

# Patch dosyasi olustur
Write-Host "Patch dosyasi yaziliyor..." -ForegroundColor Cyan

$patchStream = [System.IO.File]::Create($patchFullPath)
$writer = New-Object System.IO.BinaryWriter($patchStream)

try {
    # Header: Magic number (4 byte), Eski dosya boyutu (4 byte), Yeni dosya boyutu (4 byte)
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes("MYPT"))  # Magic: "MYPT" = My Patch
    $writer.Write([int32]$oldLen)
    $writer.Write([int32]$newLen)
    
    # Degisiklik sayisi (4 byte)
    $writer.Write([int32]$changes.Count)
    
    # Her degisiklik
    foreach ($change in $changes) {
        $writer.Write([int32]$change.Offset)      # Offset (4 byte)
        $writer.Write([int32]$change.OldLength)   # Eski uzunluk (4 byte)
        $writer.Write([int32]$change.NewLength)   # Yeni uzunluk (4 byte)
        
        # Yeni data
        if ($change.NewLength -gt 0) {
            $writer.Write($change.NewData)
        }
    }
    
    Write-Host "Patch dosyasi olusturuldu!" -ForegroundColor Green
}
finally {
    $writer.Close()
    $patchStream.Close()
}

# Patch boyutu
$patchSize = (Get-Item $patchFullPath).Length / 1MB
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

# Windows Explorer'da goster
try {
    Start-Process "explorer.exe" -ArgumentList "/select,`"$patchFullPath`""
}
catch {
    # Ignore
}

Write-Host "Cikmak icin bir tusa basin..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

