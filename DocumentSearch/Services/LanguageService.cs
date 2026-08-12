using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace DocumentSearch.Services;

public class LanguageService
{
    private const string SettingsFileName = "language.json";
    private readonly string _settingsPath;
    private string _currentLanguage = "tr";

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                ApplyLanguage(_currentLanguage);
                SaveLanguagePreference(_currentLanguage);
                LanguageChanged?.Invoke(this, _currentLanguage);
            }
        }
    }

    public event EventHandler<string>? LanguageChanged;

    public LanguageService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "E-Student");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, SettingsFileName);

        _currentLanguage = LoadLanguagePreference();
        ApplyLanguage(_currentLanguage);
    }

    public void SetLanguage(string langCode)
    {
        if (langCode == "tr" || langCode == "en" || langCode == "az")
        {
            CurrentLanguage = langCode;
        }
    }

    public void ApplyLanguage(string langCode)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            string relativePath = langCode switch
            {
                "en" => "Resources/Languages/Strings.en.xaml",
                "az" => "Resources/Languages/Strings.az.xaml",
                _ => "Resources/Languages/Strings.tr.xaml"
            };

            var langUri = new Uri(relativePath, UriKind.Relative);
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

            // Remove existing language dictionary if present
            var existingLangDict = mergedDictionaries.FirstOrDefault(d =>
                d.Source != null && d.Source.OriginalString.Contains("Strings."));

            if (existingLangDict != null)
            {
                mergedDictionaries.Remove(existingLangDict);
            }

            mergedDictionaries.Add(new ResourceDictionary { Source = langUri });
        });
    }

    private string LoadLanguagePreference()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Language", out var prop))
                {
                    var lang = prop.GetString();
                    if (lang == "tr" || lang == "en" || lang == "az")
                    {
                        return lang;
                    }
                }
            }
        }
        catch
        {
            // Fallback to Turkish
        }
        return "tr";
    }

    private void SaveLanguagePreference(string langCode)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { Language = langCode });
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Ignore write errors
        }
    }

    public string GetString(string key, params object[] args)
    {
        try
        {
            if (Application.Current?.Resources.Contains(key) == true)
            {
                var val = Application.Current.Resources[key]?.ToString() ?? key;
                if (args.Length > 0)
                {
                    return string.Format(val, args);
                }
                return val;
            }
        }
        catch { }
        return key;
    }
}
