using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using DeltaCompressionDotNet;
using DeltaCompressionDotNet.MsDelta;
using DocumentSearch.Views;

namespace DocumentSearch.Services;

/// <summary>
/// Uygulama güncelleme servisi - GitHub Releases API kullanarak sürüm kontrolü yapar
/// </summary>
public class UpdateService
{
    private readonly HttpClient _httpClient;
    private readonly string _currentVersion;
    private readonly string _githubRepoOwner; // Örnek: "kullaniciadi"
    private readonly string _githubRepoName;  // Örnek: "DocumentSearch"
    
    // GitHub Releases API URL'i
    private string LatestReleaseUrl => $"https://api.github.com/repos/{_githubRepoOwner}/{_githubRepoName}/releases/latest";
    
    // Güncelleme durumu
    public bool HasUpdate { get; private set; }
    public string? LatestVersion { get; private set; }
    
    // Güncelleme durumu değiştiğinde tetiklenen event
    public event EventHandler? UpdateStatusChanged;
    
    public UpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DocumentSearch-Updater");
        
        // Mevcut sürümü Assembly'den al
        _currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        
        // TODO: GitHub repository bilgilerinizi buraya ekleyin
        _githubRepoOwner = "rzayevsahil"; // Değiştirin
        _githubRepoName = "E-Student"; // Değiştirin
    }
    
    /// <summary>
    /// Güncelleme kontrolü yapar ve varsa kullanıcıya bildirir
    /// </summary>
    public async Task CheckForUpdatesAsync(bool silent = false)
    {
        try
        {
            var latestVersion = await GetLatestVersionAsync();
            
            if (latestVersion == null)
            {
                // Güncelleme kontrolü yapılamadı, durumu güncelle
                HasUpdate = false;
                LatestVersion = null;
                UpdateStatusChanged?.Invoke(this, EventArgs.Empty);
                
                if (!silent)
                {
                    MessageBox.Show(
                        "Güncelleme kontrolü yapılamadı. İnternet bağlantınızı kontrol edin.",
                        "Güncelleme Kontrolü",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                return;
            }
            
            // Güncelleme durumunu kontrol et ve güncelle
            bool hasUpdate = IsNewerVersion(latestVersion, _currentVersion);
            HasUpdate = hasUpdate;
            LatestVersion = latestVersion;
            UpdateStatusChanged?.Invoke(this, EventArgs.Empty);
            
            if (hasUpdate)
            {
                // Yeni sürüm bulundu - Sessiz modda sadece durumu güncelle, manuel kontrol için MessageBox göster
                if (!silent)
                {
                    var result = MessageBox.Show(
                        $"Yeni bir sürüm mevcut!\n\n" +
                        $"Mevcut Sürüm: {_currentVersion}\n" +
                        $"Yeni Sürüm: {latestVersion}\n\n" +
                        $"Güncellemeyi şimdi indirmek ister misiniz?",
                        "Yeni Güncelleme Mevcut",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        await DownloadAndInstallUpdateAsync();
                    }
                }
            }
            else
            {
                if (!silent)
                {
                    MessageBox.Show(
                        $"Uygulamanız güncel!\n\nMevcut Sürüm: {_currentVersion}",
                        "Güncelleme Kontrolü",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            // Hata durumunda durumu sıfırla
            HasUpdate = false;
            LatestVersion = null;
            UpdateStatusChanged?.Invoke(this, EventArgs.Empty);
            
            if (!silent)
            {
                MessageBox.Show(
                    $"Güncelleme kontrolü sırasında hata oluştu:\n{ex.Message}",
                    "Hata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
    
    /// <summary>
    /// GitHub'dan en son sürüm bilgisini alır
    /// </summary>
    private async Task<string?> GetLatestVersionAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(LatestReleaseUrl);
            var jsonDoc = JsonDocument.Parse(response);
            
            // GitHub API'den tag_name (sürüm numarası) al
            if (jsonDoc.RootElement.TryGetProperty("tag_name", out var tagName))
            {
                // "v1.0.1" gibi formatlardan "1.0.1" almak için
                var version = tagName.GetString()?.TrimStart('v', 'V');
                return version;
            }
        }
        catch
        {
            // Hata durumunda null döndür
        }
        
        return null;
    }
    
    /// <summary>
    /// İki sürüm numarasını karşılaştırır
    /// </summary>
    private bool IsNewerVersion(string newVersion, string currentVersion)
    {
        try
        {
            var newVer = new Version(newVersion);
            var currentVer = new Version(currentVersion);
            return newVer > currentVer;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Güncellemeyi indirir ve kurar (delta güncelleme desteği ile)
    /// </summary>
    public async Task DownloadAndInstallUpdateAsync()
    {
        UpdateDownloadWindow? downloadWindow = null;
        
        try
        {
            // İndirme penceresini göster
            Application.Current.Dispatcher.Invoke(() =>
            {
                downloadWindow = new UpdateDownloadWindow();
                downloadWindow.Show();
            });
            
            // GitHub Releases API'den assets listesini al
            var response = await _httpClient.GetStringAsync(LatestReleaseUrl);
            var jsonDoc = JsonDocument.Parse(response);
            
            if (!jsonDoc.RootElement.TryGetProperty("assets", out var assets))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    downloadWindow?.Close();
                    MessageBox.Show("Güncelleme dosyası bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return;
            }
            
            // Patch dosyası adını oluştur (örn: v2.1.4-to-v2.1.5.patch)
            var patchFileName = $"v{_currentVersion}-to-v{LatestVersion}.patch";
            string? patchUrl = null;
            string? patchFileNameFromAsset = null;
            long? patchFileSize = null;
            
            // Tam exe dosyası bilgileri (fallback için)
            string? fullExeUrl = null;
            string? fullExeFileName = null;
            long? fullExeFileSize = null;
            
            // Assets listesini kontrol et
            foreach (var asset in assets.EnumerateArray())
            {
                if (asset.TryGetProperty("browser_download_url", out var url) && 
                    asset.TryGetProperty("name", out var name))
                {
                    var urlString = url.GetString();
                    var nameString = name.GetString();
                    
                    if (urlString == null || nameString == null) continue;
                    
                    // Patch dosyası var mı? (.patch)
                    if (nameString.Equals(patchFileName, StringComparison.OrdinalIgnoreCase) || 
                        nameString.EndsWith(".patch", StringComparison.OrdinalIgnoreCase))
                    {
                        patchUrl = urlString;
                        patchFileNameFromAsset = nameString;
                        if (asset.TryGetProperty("size", out var size))
                        {
                            patchFileSize = size.GetInt64();
                        }
                    }
                    // Tam exe dosyası (fallback için)
                    else if (nameString.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        fullExeUrl = urlString;
                        fullExeFileName = nameString;
                        if (asset.TryGetProperty("size", out var size))
                        {
                            fullExeFileSize = size.GetInt64();
                        }
                    }
                }
            }
            
            var tempPath = Path.GetTempPath();
            string? updateFilePath = null;
            bool usePatch = false;
            
            // Patch dosyası varsa delta güncelleme kullan
            if (patchUrl != null && patchFileNameFromAsset != null)
            {
                usePatch = true;
                var patchFilePath = Path.Combine(tempPath, patchFileNameFromAsset);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    downloadWindow?.SetStatus($"Delta güncelleme indiriliyor: {patchFileNameFromAsset} ({(patchFileSize.HasValue ? (patchFileSize.Value / 1024.0 / 1024.0).ToString("F2") : "?")} MB)");
                });
                
                // Patch dosyasını indir
                using (var responseStream = await _httpClient.GetStreamAsync(patchUrl))
                using (var fileStream = new FileStream(patchFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[8192];
                    long totalBytesRead = 0;
                    int bytesRead;
                    
                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        
                        if (patchFileSize.HasValue)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                downloadWindow?.UpdateProgress(totalBytesRead, patchFileSize.Value);
                            });
                        }
                    }
                }
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    downloadWindow?.SetStatus("Patch uygulanıyor...");
                });
                
                // Mevcut exe'nin yolunu al
                var currentExePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(currentExePath) || !File.Exists(currentExePath))
                {
                    throw new FileNotFoundException("Mevcut uygulama dosyası bulunamadı.");
                }
                
                // Yeni exe dosyası yolu
                var currentExeDir = Path.GetDirectoryName(currentExePath);
                var currentExeName = Path.GetFileNameWithoutExtension(currentExePath);
                updateFilePath = Path.Combine(tempPath, $"{currentExeName}_New_{LatestVersion}.exe");
                
                // Patch'i uygula
                try
                {
                    // Patch dosyasının magic number'ını kontrol et
                    // "MYPT" ise kendi patch sistemimizi kullan, değilse MsDeltaCompression
                    bool useMyPatch = false;
                    try
                    {
                        using (var patchStream = File.OpenRead(patchFilePath))
                        using (var reader = new BinaryReader(patchStream))
                        {
                            var magic = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4));
                            if (magic == "MYPT")
                            {
                                useMyPatch = true;
                            }
                        }
                    }
                    catch
                    {
                        // Magic number okunamazsa MsDeltaCompression dene
                        useMyPatch = false;
                    }
                    
                    if (useMyPatch)
                    {
                        // Kendi patch sistemimizi kullan
                        MyPatchService.ApplyPatch(currentExePath, patchFilePath, updateFilePath);
                    }
                    else
                    {
                        // Normal patch dosyası için MsDeltaCompression kullan
                        var deltaCompression = new MsDeltaCompression();
                        deltaCompression.ApplyDelta(currentExePath, patchFilePath, updateFilePath);
                    }
                }
                catch (Exception patchEx)
                {
                    // Patch uygulama başarısız olursa tam exe'ye geç
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        downloadWindow?.SetStatus("Patch uygulanamadı, tam güncelleme indiriliyor...");
                    });
                    
                    usePatch = false;
                    File.Delete(patchFilePath);
                    
                    if (fullExeUrl == null)
                    {
                        throw new Exception("Patch uygulanamadı ve tam güncelleme dosyası bulunamadı.", patchEx);
                    }
                }
            }
            
            // Patch yoksa veya başarısız olduysa tam exe indir
            if (!usePatch)
            {
                if (fullExeUrl == null || fullExeFileName == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        downloadWindow?.Close();
                        MessageBox.Show("Güncelleme dosyası bulunamadı. Lütfen manuel olarak indirin.", 
                            "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    return;
                }
                
                updateFilePath = Path.Combine(tempPath, fullExeFileName);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    downloadWindow?.SetStatus($"Güncelleme dosyası indiriliyor: {fullExeFileName} ({(fullExeFileSize.HasValue ? (fullExeFileSize.Value / 1024.0 / 1024.0).ToString("F2") : "?")} MB)");
                });
                
                // Tam exe'yi indir
                using (var responseStream = await _httpClient.GetStreamAsync(fullExeUrl))
                using (var fileStream = new FileStream(updateFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[8192];
                    long totalBytesRead = 0;
                    int bytesRead;
                    
                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        
                        if (fullExeFileSize.HasValue)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                downloadWindow?.UpdateProgress(totalBytesRead, fullExeFileSize.Value);
                            });
                        }
                    }
                }
            }
            
            // İndirme/Uygulama tamamlandı
            Application.Current.Dispatcher.Invoke(() =>
            {
                downloadWindow?.SetStatus("Güncelleme hazır. Uygulama kapatılıyor...");
                downloadWindow?.UpdateProgress(100, 100);
            });
            
            await Task.Delay(1000);
            
            // Mevcut exe'nin yolunu al
            var currentExePathForReplace = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExePathForReplace))
            {
                throw new FileNotFoundException("Mevcut uygulama dosyası bulunamadı.");
            }
            
            // updateFilePath kontrolü
            if (string.IsNullOrEmpty(updateFilePath) || !File.Exists(updateFilePath))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    downloadWindow?.Close();
                    MessageBox.Show("Güncelleme dosyası oluşturulamadı.", 
                        "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return;
            }
            
            // Exe değiştirme batch script'i oluştur
            // Bu script uygulama kapandıktan sonra exe'yi değiştirecek
            var currentExeDir = Path.GetDirectoryName(currentExePathForReplace);
            var currentExeName = Path.GetFileNameWithoutExtension(currentExePathForReplace);
            var currentExeExt = Path.GetExtension(currentExePathForReplace);
            var currentExeFileName = Path.GetFileName(currentExePathForReplace);
            var newExeName = $"{currentExeName}_New{currentExeExt}";
            var newExePath = Path.Combine(currentExeDir, newExeName);
            
            // Yeni exe'yi geçici isimle kaydet (aynı klasörde)
            File.Copy(updateFilePath, newExePath, true);
            
            // Eski exe'yi yedekle
            var backupPath = currentExePathForReplace + ".old";
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
            
            // Update batch script oluştur
            var updateScript = Path.Combine(Path.GetTempPath(), $"DocumentSearch_Update_{Guid.NewGuid()}.bat");
            var scriptContent = $@"@echo off
REM Uygulamanin kapanmasini bekle
:WAIT
tasklist /FI ""IMAGENAME eq {currentExeFileName}"" 2>NUL | find /I /N ""{currentExeFileName}"">NUL
if ""%ERRORLEVEL%""==""0"" (
    timeout /t 1 /nobreak >nul
    goto WAIT
)

REM Kisa bir bekleme daha (dosya kilidi kalkmasi icin)
timeout /t 2 /nobreak >nul

REM Eski exe'yi yedekle
if exist ""{currentExePathForReplace}"" (
    copy ""{currentExePathForReplace}"" ""{backupPath}"" >nul 2>&1
)

REM Eski exe'yi sil
del ""{currentExePathForReplace}"" >nul 2>&1

REM Yeni exe'yi eski isme tasi
move ""{newExePath}"" ""{currentExePathForReplace}"" >nul 2>&1

REM Yeni exe'yi baslat
start """" ""{currentExePathForReplace}""

REM Yedek ve temp dosyalarini temizle
timeout /t 3 /nobreak >nul
del ""{backupPath}"" >nul 2>&1
del ""{updateFilePath}"" >nul 2>&1
del ""{updateScript}"" >nul 2>&1";
            
            File.WriteAllText(updateScript, scriptContent);
            
            // İndirme penceresini kapat
            Application.Current.Dispatcher.Invoke(() =>
            {
                downloadWindow?.Close();
            });
            
            // Update script'i çalıştır (arka planda)
            var updateProcess = new ProcessStartInfo
            {
                FileName = updateScript,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };
            Process.Start(updateProcess);
            
            // Uygulamayı kapat
            await Task.Delay(500);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                downloadWindow?.Close();
                MessageBox.Show(
                    $"Güncelleme indirme sırasında hata oluştu:\n{ex.Message}",
                    "Hata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }
    }
    
    /// <summary>
    /// GitHub Releases sayfasını tarayıcıda açar
    /// </summary>
    public void OpenReleasesPage()
    {
        var url = $"https://github.com/{_githubRepoOwner}/{_githubRepoName}/releases";
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}

