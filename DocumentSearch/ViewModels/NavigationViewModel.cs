using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentSearch.Services;
using DocumentSearch.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace DocumentSearch.ViewModels;

public partial class NavigationViewModel : ObservableObject
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly UpdateService? _updateService;
    private DispatcherTimer? _updateCheckTimer;

    [ObservableProperty]
    private UserControl? currentView;

    [ObservableProperty]
    private string selectedMenuItem = "DocumentSearch";

    [ObservableProperty]
    private string updateButtonText = "✨ Yeni Güncelleme Mevcut";

    [ObservableProperty]
    private string updateButtonForeground = "#FF9800";

    [ObservableProperty]
    private bool isUpdateAvailable = false;

    [ObservableProperty]
    private string currentVersion = GetCurrentVersion();

    public NavigationViewModel(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
        _updateService = serviceProvider?.GetService<UpdateService>();
        
        // Güncelleme durumu değiştiğinde buton metnini güncelle
        if (_updateService != null)
        {
            _updateService.UpdateStatusChanged += UpdateService_UpdateStatusChanged;
            
            // İlk kontrolü yap (3 saniye sonra)
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000); // 3 saniye bekle
                await _updateService.CheckForUpdatesAsync(silent: true);
            });
            
            // Periyodik güncelleme kontrolü başlat (her 30 dakikada bir)
            StartPeriodicUpdateCheck();
        }
        
        NavigateToDocumentSearch();
    }

    /// <summary>
    /// Periyodik güncelleme kontrolünü başlatır (her 30 dakikada bir)
    /// </summary>
    private void StartPeriodicUpdateCheck()
    {
        if (_updateService == null)
            return;

        _updateCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromHours(12) // Her 30 dakikada bir kontrol et
        };
        
        _updateCheckTimer.Tick += async (sender, e) =>
        {
            if (_updateService != null)
            {
                await _updateService.CheckForUpdatesAsync(silent: true);
            }
        };
        
        _updateCheckTimer.Start();
    }

    private void UpdateService_UpdateStatusChanged(object? sender, EventArgs e)
    {
        if (_updateService != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_updateService.HasUpdate)
                {
                    UpdateButtonText = $"✨ Yeni Güncelleme Mevcut (v{_updateService.LatestVersion})";
                    UpdateButtonForeground = "#FF9800"; // Turuncu renk
                    IsUpdateAvailable = true; // Butonu göster
                }
                else
                {
                    IsUpdateAvailable = false; // Butonu gizle
                }
            });
        }
    }

    [RelayCommand]
    private void NavigateToDocumentSearch()
    {
        SelectedMenuItem = "DocumentSearch";
        var view = new DocumentSearchView();
        if (_serviceProvider != null)
        {
            var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            view.DataContext = viewModel;
        }
        CurrentView = view;
    }

    [RelayCommand]
    private void NavigateToPomodoro()
    {
        SelectedMenuItem = "Pomodoro";
        var view = new PomodoroView();
        if (_serviceProvider != null)
        {
            var viewModel = _serviceProvider.GetRequiredService<PomodoroViewModel>();
            view.DataContext = viewModel;
        }
        CurrentView = view;
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        if (_updateService != null)
        {
            await _updateService.CheckForUpdatesAsync(silent: false);
        }
    }

    /// <summary>
    /// Mevcut sürüm bilgisini alır (son .0 kısmını kaldırır)
    /// </summary>
    private static string GetCurrentVersion()
    {
        try
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                // Eğer Revision (4. parça) 0 ise, sadece Major.Minor.Build göster
                if (version.Revision == 0)
                {
                    return $"v{version.Major}.{version.Minor}.{version.Build}";
                }
                else
                {
                    return $"v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
            }
            return "v1.0.0";
        }
        catch
        {
            return "v1.0.0";
        }
    }
}

