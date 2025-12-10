using DocumentSearch.ViewModels;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;

namespace DocumentSearch;

public partial class MainWindow : Window
{
    public MainWindow(NavigationViewModel navigationViewModel)
    {
        InitializeComponent();
        DataContext = navigationViewModel;
        // Logo dosyası varsa yükle, yoksa gizle
        LoadLogo();
    }

    private void LoadLogo()
    {
        try
        {
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.png");
            if (File.Exists(logoPath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new System.Uri(logoPath, System.UriKind.Absolute);
                bitmap.EndInit();
                LogoImage.Source = bitmap;
                LogoImage.Visibility = Visibility.Visible;
            }
            else
            {
                LogoImage.Visibility = Visibility.Collapsed;
            }
        }
        catch
        {
            // Logo yüklenemezse gizle
            LogoImage.Visibility = Visibility.Collapsed;
        }
    }
}
