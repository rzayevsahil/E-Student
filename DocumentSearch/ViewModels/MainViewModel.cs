using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentSearch.Models;
using DocumentSearch.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace DocumentSearch.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDocumentService _documentService;
    private readonly ISearchService _searchService;

    [ObservableProperty]
    private ObservableCollection<Document> documents = new();

    [ObservableProperty]
    private ObservableCollection<SearchResult> searchResults = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Hazır";

    public MainViewModel(IDocumentService documentService, ISearchService searchService)
    {
        _documentService = documentService;
        _searchService = searchService;
        
        // Uygulama başlarken kayıtlı dosyaları yükle
        _ = InitializeAsync();
        
        // SearchQuery değiştiğinde otomatik arama yap
        PropertyChanged += MainViewModel_PropertyChanged;
    }
    
    private void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchQuery))
        {
            // Kısa bir gecikme ile arama yap (kullanıcı yazmayı bitirsin)
            _ = Task.Delay(300).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PerformSearch();
                });
            });
        }
    }
    
    private async Task InitializeAsync()
    {
        IsLoading = true;
        StatusMessage = "Kayıtlı dosyalar yükleniyor...";
        
        try
        {
            await _documentService.LoadSavedDocumentsAsync();
            var allDocuments = _documentService.GetAllDocuments();
            
            foreach (var doc in allDocuments)
            {
                Documents.Add(doc);
            }
            
            StatusMessage = $"{allDocuments.Count} kayıtlı dosya yüklendi.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadFiles()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Dosyalar|*.pdf;*.xlsx;*.xls;*.docx;*.doc|PDF Dosyaları|*.pdf|Excel Dosyaları|*.xlsx;*.xls|Word Dosyaları|*.docx;*.doc|Tüm Dosyalar|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true)
        {
            IsLoading = true;
            StatusMessage = "Dosyalar yükleniyor...";

            try
            {
                foreach (var filePath in dialog.FileNames)
                {
                    // Eğer dosya zaten yüklenmişse, atla
                    if (Documents.Any(d => d.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                        continue;
                        
                    var document = await _documentService.LoadDocumentAsync(filePath);
                    Documents.Add(document);
                }

                StatusMessage = $"{dialog.FileNames.Length} dosya yüklendi. Toplam {Documents.Count} dosya.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Hata: {ex.Message}";
                // Hata detaylarını göster
                System.Windows.MessageBox.Show(
                    $"Dosya yüklenirken hata oluştu:\n\n{ex.Message}\n\nDetay: {ex.InnerException?.Message ?? "Yok"}",
                    "Hata",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private void RemoveDocument(Document? document)
    {
        if (document == null)
            return;

        _documentService.RemoveDocument(document.FilePath);
        Documents.Remove(document);
        StatusMessage = $"{document.FileName} kaldırıldı.";
        
        // Arama sonuçlarını güncelle
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            PerformSearch();
        }
    }


    private void PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchResults.Clear();
            StatusMessage = "";
            return;
        }

        var allDocuments = _documentService.GetAllDocuments();
        
        // Eğer hiç doküman yoksa
        if (allDocuments == null || !allDocuments.Any())
        {
            SearchResults.Clear();
            StatusMessage = "Yüklenmiş dosya yok.";
            return;
        }
        
        var results = _searchService.Search(SearchQuery.Trim(), allDocuments);
        
        SearchResults.Clear();
        foreach (var result in results)
        {
            SearchResults.Add(result);
        }

        StatusMessage = $"{results.Count} sonuç bulundu.";
    }

}

