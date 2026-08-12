using DocumentSearch.Services;
using DocumentSearch.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Threading.Tasks;

namespace DocumentSearch;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show($"Beklenmeyen Hata:\n\n{args.Exception.Message}\n\nDetay:\n{args.Exception.StackTrace}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        // NavigationViewModel'i oluştururken ServiceProvider'ı geçir
        var navigationViewModel = new NavigationViewModel(_serviceProvider);
        var mainWindow = new MainWindow(navigationViewModel);
        mainWindow.Show();
        
        // Not: Güncelleme kontrolü NavigationViewModel constructor'ında yapılıyor
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<IOcrService, OcrService>();
        services.AddSingleton<IPdfParser, PdfParser>();
        services.AddSingleton<IExcelParser, ExcelParser>();
        services.AddSingleton<IWordParser, WordParser>();
        services.AddSingleton<IPowerPointParser, PowerPointParser>();
        services.AddSingleton<IDocumentService, DocumentService>();
        services.AddSingleton<ISearchService, SearchService>();
        services.AddSingleton<UpdateService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<LanguageService>();
        services.AddSingleton<PomodoroService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<PomodoroViewModel>();
        // NavigationViewModel'i singleton olarak kaydetme, constructor'da ServiceProvider geçireceğiz

        // Views
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
