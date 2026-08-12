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
    private ObservableCollection<Document> filteredDocuments = new();

    [ObservableProperty]
    private ObservableCollection<SearchResult> searchResults = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string selectedFileFilter = "All"; // All, PDF, Word, Excel, PowerPoint

    [ObservableProperty]
    private string documentFilterText = string.Empty;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private int pdfCount;

    [ObservableProperty]
    private int wordCount;

    [ObservableProperty]
    private int excelCount;

    [ObservableProperty]
    private int pptCount;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Hazır";

    private CancellationTokenSource? _searchCts;

    public MainViewModel(IDocumentService documentService, ISearchService searchService)
    {
        _documentService = documentService;
        _searchService = searchService;
        
        // Uygulama başlarken kayıtlı dosyaları yükle
        _ = InitializeAsync();
        
        // SearchQuery veya Filtre değiştiğinde otomatik güncelle
        PropertyChanged += MainViewModel_PropertyChanged;
    }
    
    private void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchQuery))
        {
            _ = PerformSearchAsync();
        }
        else if (e.PropertyName == nameof(SelectedFileFilter) || e.PropertyName == nameof(DocumentFilterText))
        {
            UpdateFilteredDocuments();
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
            
            Documents.Clear();
            foreach (var doc in allDocuments)
            {
                Documents.Add(doc);
            }
            
            UpdateFilteredDocuments();
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
    private void SelectFilter(string filterName)
    {
        if (!string.IsNullOrEmpty(filterName))
        {
            SelectedFileFilter = filterName;
        }
    }

    public void UpdateFilteredDocuments()
    {
        TotalCount = Documents.Count;
        PdfCount = Documents.Count(d => d.FileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase));
        WordCount = Documents.Count(d => d.FileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase) || d.FileExtension.Equals(".doc", StringComparison.OrdinalIgnoreCase));
        ExcelCount = Documents.Count(d => d.FileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) || d.FileExtension.Equals(".xls", StringComparison.OrdinalIgnoreCase));
        PptCount = Documents.Count(d => d.FileExtension.Equals(".pptx", StringComparison.OrdinalIgnoreCase) || d.FileExtension.Equals(".ppt", StringComparison.OrdinalIgnoreCase));

        IEnumerable<Document> docs = Documents;

        if (!string.IsNullOrWhiteSpace(DocumentFilterText))
        {
            var filter = DocumentFilterText.Trim().ToLower();
            docs = docs.Where(d => d.FileName.ToLower().Contains(filter));
        }

        if (SelectedFileFilter != "All")
        {
            docs = SelectedFileFilter switch
            {
                "PDF" => docs.Where(d => d.FileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)),
                "Word" => docs.Where(d => d.FileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase) || d.FileExtension.Equals(".doc", StringComparison.OrdinalIgnoreCase)),
                "Excel" => docs.Where(d => d.FileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) || d.FileExtension.Equals(".xls", StringComparison.OrdinalIgnoreCase)),
                "PowerPoint" => docs.Where(d => d.FileExtension.Equals(".pptx", StringComparison.OrdinalIgnoreCase) || d.FileExtension.Equals(".ppt", StringComparison.OrdinalIgnoreCase)),
                _ => docs
            };
        }

        FilteredDocuments.Clear();
        foreach (var doc in docs)
        {
            FilteredDocuments.Add(doc);
        }
    }

    [RelayCommand]
    private async Task LoadFiles()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Desteklenen Dosyalar|*.pdf;*.xlsx;*.xls;*.docx;*.doc;*.pptx;*.ppt|PDF Dosyaları|*.pdf|Excel Dosyaları|*.xlsx;*.xls|Word Dosyaları|*.docx;*.doc|PowerPoint Sunumları|*.pptx;*.ppt|Tüm Dosyalar|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true)
        {
            IsLoading = true;
            StatusMessage = "Dosyalar yükleniyor...";

            try
            {
                var filesToLoad = dialog.FileNames
                    .Where(path => !Documents.Any(d => d.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (filesToLoad.Any())
                {
                    // Dosyaları paralel olarak yükle
                    var tasks = filesToLoad.Select(filePath => _documentService.LoadDocumentAsync(filePath));
                    var loadedDocs = await Task.WhenAll(tasks);

                    foreach (var doc in loadedDocs)
                    {
                        if (!Documents.Any(d => d.FilePath.Equals(doc.FilePath, StringComparison.OrdinalIgnoreCase)))
                        {
                            Documents.Add(doc);
                        }
                    }
                    UpdateFilteredDocuments();
                }

                StatusMessage = $"{dialog.FileNames.Length} dosya işlendi. Toplam {Documents.Count} dosya.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Hata: {ex.Message}";
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

    public async Task LoadFilesFromPathsAsync(IEnumerable<string> filePaths)
    {
        var validExtensions = new[] { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt" };
        var filesToLoad = filePaths
            .Where(path => validExtensions.Contains(Path.GetExtension(path).ToLower()))
            .Where(path => !Documents.Any(d => d.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (!filesToLoad.Any()) return;

        IsLoading = true;
        StatusMessage = "Sürüklenen dosyalar yükleniyor...";

        try
        {
            var tasks = filesToLoad.Select(filePath => _documentService.LoadDocumentAsync(filePath));
            var loadedDocs = await Task.WhenAll(tasks);

            foreach (var doc in loadedDocs)
            {
                if (!Documents.Any(d => d.FilePath.Equals(doc.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    Documents.Add(doc);
                }
            }

            UpdateFilteredDocuments();
            StatusMessage = $"{filesToLoad.Count} dosya eklendi. Toplam {Documents.Count} dosya.";
            
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                _ = PerformSearchAsync();
            }
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
    private void RemoveDocument(Document? document)
    {
        if (document == null)
            return;

        _documentService.RemoveDocument(document.FilePath);
        Documents.Remove(document);
        UpdateFilteredDocuments();
        StatusMessage = $"{document.FileName} kaldırıldı.";
        
        // Arama sonuçlarını güncelle
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            _ = PerformSearchAsync();
        }
    }

    private async Task PerformSearchAsync()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            // Kullanıcı yazmayı tamamlasın diye 250ms gecikme (Debounce)
            await Task.Delay(250, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        var query = SearchQuery?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            SearchResults.Clear();
            StatusMessage = "";
            return;
        }

        var allDocuments = _documentService.GetAllDocuments();
        
        if (allDocuments == null || !allDocuments.Any())
        {
            SearchResults.Clear();
            StatusMessage = "Yüklenmiş dosya yok.";
            return;
        }

        // Aramayı arka planda paralel çalıştır
        var results = await Task.Run(() => _searchService.Search(query, allDocuments), token);

        if (token.IsCancellationRequested) return;

        SearchResults.Clear();
        foreach (var result in results)
        {
            SearchResults.Add(result);
        }

        StatusMessage = $"{results.Count} sonuç bulundu.";
    }

}

