using System.IO;
using System.Text.Json;
using System.Windows;

namespace DocumentSearch.Services;

public class ThemeService
{
    private const string SettingsFileName = "theme.json";
    private readonly string _settingsPath;
    private bool _isDarkMode;

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                ApplyTheme(_isDarkMode);
                SaveThemePreference(_isDarkMode);
                ThemeChanged?.Invoke(this, _isDarkMode);
            }
        }
    }

    public event EventHandler<bool>? ThemeChanged;

    public ThemeService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "E-Student");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, SettingsFileName);

        _isDarkMode = LoadThemePreference();
        ApplyTheme(_isDarkMode);
    }

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
    }

    private void ApplyTheme(bool isDark)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var themeUri = isDark
                ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            
            // Remove existing theme dictionaries if any
            var existingTheme = mergedDictionaries.FirstOrDefault(d => 
                d.Source != null && (d.Source.OriginalString.Contains("DarkTheme") || d.Source.OriginalString.Contains("LightTheme")));

            if (existingTheme != null)
            {
                mergedDictionaries.Remove(existingTheme);
            }

            mergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
        });
    }

    private bool LoadThemePreference()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("IsDarkMode", out var prop))
                {
                    return prop.GetBoolean();
                }
            }
        }
        catch
        {
            // Fallback to light mode on error
        }
        return false;
    }

    private void SaveThemePreference(bool isDark)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { IsDarkMode = isDark });
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Ignore write errors
        }
    }
}
