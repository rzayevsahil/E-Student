using DocumentSearch.Services;
using DocumentSearch.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DocumentSearch;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        // NavigationViewModel'i oluştururken ServiceProvider'ı geçir
        var navigationViewModel = new NavigationViewModel(_serviceProvider);
        var mainWindow = new MainWindow(navigationViewModel);
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<IPdfParser, PdfParser>();
        services.AddSingleton<IExcelParser, ExcelParser>();
        services.AddSingleton<IWordParser, WordParser>();
        services.AddSingleton<IPdfToExcelConverter, PdfToExcelConverter>();
        services.AddSingleton<IDocumentService, DocumentService>();
        services.AddSingleton<ISearchService, SearchService>();

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
