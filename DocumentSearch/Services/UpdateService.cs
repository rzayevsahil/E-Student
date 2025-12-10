using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

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
            
            if (IsNewerVersion(latestVersion, _currentVersion))
            {
                // Yeni sürüm bulundu
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
    /// Güncellemeyi indirir ve kurar
    /// </summary>
    private async Task DownloadAndInstallUpdateAsync()
    {
        try
        {
            // GitHub Releases API'den indirme URL'ini al
            var response = await _httpClient.GetStringAsync(LatestReleaseUrl);
            var jsonDoc = JsonDocument.Parse(response);
            
            if (!jsonDoc.RootElement.TryGetProperty("assets", out var assets))
            {
                MessageBox.Show("Güncelleme dosyası bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Setup.exe veya .msi dosyasını bul
            string? downloadUrl = null;
            string? fileName = null;
            
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
                        break;
                    }
                }
            }
            
            if (downloadUrl == null)
            {
                MessageBox.Show("Güncelleme dosyası bulunamadı. Lütfen manuel olarak indirin.", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Kullanıcıya indirme konumunu sor
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = fileName ?? "DocumentSearch_Update.exe",
                Filter = "Executable Files|*.exe|All Files|*.*"
            };
            
            if (saveDialog.ShowDialog() == true)
            {
                // Dosyayı indir
                var fileBytes = await _httpClient.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(saveDialog.FileName, fileBytes);
                
                // İndirilen dosyayı çalıştır
                Process.Start(new ProcessStartInfo
                {
                    FileName = saveDialog.FileName,
                    UseShellExecute = true
                });
                
                // Uygulamayı kapat (güncelleme kurulumu başlatıldı)
                Application.Current.Shutdown();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Güncelleme indirme sırasında hata oluştu:\n{ex.Message}",
                "Hata",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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

