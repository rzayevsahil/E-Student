using DocumentSearch.Services;
using DocumentSearch.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace DocumentSearch;

public partial class App : Application
{
    private static Mutex? _mutex;
    private ServiceProvider? _serviceProvider;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsIconic(IntPtr hWnd);

    private const int SW_RESTORE = 9;

    protected override void OnStartup(StartupEventArgs e)
    {
        const string mutexName = "Global\\E-Student_SingleInstance_Mutex_D1A39F71-B88E-47C2-823E-91C083758A11";
        _mutex = new Mutex(true, mutexName, out bool createdNew);

        if (!createdNew)
        {
            BringExistingInstanceToForeground();
            Shutdown();
            return;
        }

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

    private static void BringExistingInstanceToForeground()
    {
        try
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);

            foreach (var process in processes)
            {
                if (process.Id != currentProcess.Id)
                {
                    IntPtr hWnd = process.MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        if (IsIconic(hWnd))
                        {
                            ShowWindow(hWnd, SW_RESTORE);
                        }
                        SetForegroundWindow(hWnd);
                    }
                    break;
                }
            }
        }
        catch
        {
            // Ignore errors when searching for processes
        }
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
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<PomodoroViewModel>();
        // NavigationViewModel'i singleton olarak kaydetme, constructor'da ServiceProvider geçireceğiz

        // Views
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_mutex != null)
        {
            try
            {
                _mutex.ReleaseMutex();
            }
            catch { }
            _mutex.Dispose();
        }

        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
