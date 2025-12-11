using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DocumentSearch.Views;

public partial class UpdateDownloadWindow : Window, INotifyPropertyChanged
{
    private double _downloadProgress = 0;
    private string _progressText = "0%";
    private string _statusMessage = "Güncelleme dosyası indiriliyor...";

    public double DownloadProgress
    {
        get => _downloadProgress;
        set
        {
            _downloadProgress = value;
            OnPropertyChanged();
        }
    }

    public string ProgressText
    {
        get => _progressText;
        set
        {
            _progressText = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public UpdateDownloadWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public void UpdateProgress(long bytesDownloaded, long totalBytes)
    {
        if (totalBytes > 0)
        {
            DownloadProgress = (bytesDownloaded * 100.0) / totalBytes;
            ProgressText = $"{DownloadProgress:F1}%";
        }
    }

    public void SetStatus(string message)
    {
        StatusMessage = message;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

