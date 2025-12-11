using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
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
    /// Güncellemeyi indirir ve kurar (otomatik indirme, progress gösterimi)
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
            
            // GitHub Releases API'den indirme URL'ini al
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
            
            // Setup.exe veya .msi dosyasını bul
            string? downloadUrl = null;
            string? fileName = null;
            long? fileSize = null;
            
            foreach (var asset in assets.EnumerateArray())
            {
                if (asset.TryGetProperty("browser_download_url", out var url))
                {
                    var urlString = url.GetString();
                    if (urlString != null && (urlString.EndsWith(".exe") || urlString.EndsWith(".msi")))
                    {
                        downloadUrl = urlString;
                        if (asset.TryGetProperty("name", out var name))
                        {
                            fileName = name.GetString();
                        }
                        if (asset.TryGetProperty("size", out var size))
                        {
                            fileSize = size.GetInt64();
                        }
                        break;
                    }
                }
            }
            
            if (downloadUrl == null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    downloadWindow?.Close();
                    MessageBox.Show("Güncelleme dosyası bulunamadı. Lütfen manuel olarak indirin.", 
                        "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return;
            }
            
            // Temp klasörüne indir
            var tempPath = Path.GetTempPath();
            var updateFileName = fileName ?? $"DocumentSearch_Update_{LatestVersion}.exe";
            var updateFilePath = Path.Combine(tempPath, updateFileName);
            
            // Durum mesajını güncelle
            Application.Current.Dispatcher.Invoke(() =>
            {
                downloadWindow?.SetStatus($"Güncelleme dosyası indiriliyor: {updateFileName}");
            });
            
            // İlerlemeli indirme
            using (var responseStream = await _httpClient.GetStreamAsync(downloadUrl))
            using (var fileStream = new FileStream(updateFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;
                
                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    
                    // Progress güncelle
                    if (fileSize.HasValue)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            downloadWindow?.UpdateProgress(totalBytesRead, fileSize.Value);
                        });
                    }
                }
            }
            
            // İndirme tamamlandı
            Application.Current.Dispatcher.Invoke(() =>
            {
                downloadWindow?.SetStatus("İndirme tamamlandı. Uygulama kapatılıyor...");
                downloadWindow?.UpdateProgress(100, 100);
            });
            
            // Kısa bir bekleme (kullanıcı mesajı görebilsin)
            await Task.Delay(1000);
            
            // İndirme penceresini kapat
            Application.Current.Dispatcher.Invoke(() =>
            {
                downloadWindow?.Close();
            });
            
            // Yeni exe'yi çalıştır
            var processStartInfo = new ProcessStartInfo
            {
                FileName = updateFilePath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(updateFilePath)
            };
            
            Process.Start(processStartInfo);
            
            // Uygulamayı kapat (yeni sürüm açılacak)
            await Task.Delay(500); // Yeni process'in başlaması için kısa bekleme
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

