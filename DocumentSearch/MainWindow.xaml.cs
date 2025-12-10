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
            // Önce embedded resource'dan yüklemeyi dene (publish edildiğinde çalışır)
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = "DocumentSearch.Assets.logo.png";
            
            if (assembly.GetManifestResourceNames().Contains(resourceName))
            {
                // Embedded resource'dan yükle
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze(); // Thread-safe için
                        LogoImage.Source = bitmap;
                        LogoImage.Visibility = Visibility.Visible;
                        return;
                    }
                }
            }
            
            // Embedded resource yoksa, dosya sisteminden yüklemeyi dene (debug modda)
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
