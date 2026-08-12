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
    private readonly LanguageService _languageService;
    private Action? _lastStatusUpdate;

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
    private int favoriteCount;

    [ObservableProperty]
    private int imageCount;

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
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private SearchResult? selectedSearchResult;

    [ObservableProperty]
    private bool isPreviewOpen;

    [ObservableProperty]
    private string previewDocumentName = string.Empty;

    [ObservableProperty]
    private int previewPageNumber;

    [ObservableProperty]
    private string previewSnippet = string.Empty;

    [ObservableProperty]
    private string previewPageText = string.Empty;

    [ObservableProperty]
    private string previewFileExtension = string.Empty;

    [ObservableProperty]
    private string previewDocumentPath = string.Empty;

    [ObservableProperty]
    private int previewCurrentPage = 1;

    [ObservableProperty]
    private int previewTotalPages = 1;

    private List<string> _currentPreviewPages = new();

    private CancellationTokenSource? _searchCts;

    public MainViewModel(IDocumentService documentService, ISearchService searchService, LanguageService languageService)
    {
        _documentService = documentService;
        _searchService = searchService;
        _languageService = languageService;

        _languageService.LanguageChanged += (s, lang) => _lastStatusUpdate?.Invoke();
        SetStatus(() => StatusMessage = _languageService.GetString("Status_Ready"));

        // Uygulama başlarken kayıtlı dosyaları yükle
        _ = InitializeAsync();

        // SearchQuery veya Filtre değiştiğinde otomatik güncelle
        PropertyChanged += MainViewModel_PropertyChanged;
    }

    private void SetStatus(Action action)
    {
        _lastStatusUpdate = action;
        action();
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
        else if (e.PropertyName == nameof(SelectedSearchResult))
        {
            if (SelectedSearchResult != null)
            {
                OpenSearchResultPreview(SelectedSearchResult);
            }
        }
    }

    public void OpenSearchResultPreview(SearchResult result)
    {
        if (result == null) return;

        PreviewDocumentName = result.DocumentName;
        PreviewDocumentPath = result.DocumentPath;
        PreviewFileExtension = result.FileExtension;
        PreviewSnippet = result.Snippet;

        var doc = Documents.FirstOrDefault(d => d.FilePath.Equals(result.DocumentPath, StringComparison.OrdinalIgnoreCase));
        if (doc != null)
        {
            _currentPreviewPages = ExtractPages(doc.RawContent);
            PreviewTotalPages = _currentPreviewPages.Count > 0 ? _currentPreviewPages.Count : 1;
        }
        else
        {
            _currentPreviewPages = new List<string> { result.PageText };
            PreviewTotalPages = 1;
        }

        PreviewPageNumber = result.PageNumber;
        if (result.PageNumber > 0 && result.PageNumber <= PreviewTotalPages && _currentPreviewPages.Count >= result.PageNumber)
        {
            PreviewCurrentPage = result.PageNumber;
            PreviewPageText = _currentPreviewPages[result.PageNumber - 1];
        }
        else
        {
            PreviewCurrentPage = 1;
            PreviewPageText = _currentPreviewPages.Count > 0 ? _currentPreviewPages[0] : result.PageText;
        }

        IsPreviewOpen = true;
    }

    [RelayCommand]
    private void OpenDocumentPreview(Document? doc)
    {
        if (doc == null) return;

        PreviewDocumentName = doc.FileName;
        PreviewDocumentPath = doc.FilePath;
        PreviewFileExtension = doc.FileExtension;
        PreviewSnippet = $"Dosya Adı: {doc.FileName}";

        _currentPreviewPages = ExtractPages(doc.RawContent);
        PreviewTotalPages = _currentPreviewPages.Count > 0 ? _currentPreviewPages.Count : 1;
        PreviewCurrentPage = 1;
        PreviewPageText = _currentPreviewPages.Count > 0 ? _currentPreviewPages[0] : (string.IsNullOrWhiteSpace(doc.RawContent) ? "İçerik okunamadı veya boş." : doc.RawContent);
        PreviewPageNumber = 1;

        IsPreviewOpen = true;
    }

    private List<string> ExtractPages(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent)) return new List<string>();
        var pageSeparator = "---PAGE_";
        var rawPages = rawContent.Split(new[] { pageSeparator }, StringSplitOptions.RemoveEmptyEntries);
        var pages = new List<string>();
        
        foreach (var rawPage in rawPages)
        {
            var match = System.Text.RegularExpressions.Regex.Match(rawPage, @"^(\d+)---");
            if (match.Success)
            {
                pages.Add(rawPage.Substring(match.Length).Trim());
            }
            else
            {
                pages.Add(rawPage.Trim());
            }
        }
        return pages;
    }

    [RelayCommand]
    private void ClosePreview()
    {
        IsPreviewOpen = false;
        SelectedSearchResult = null;
    }

    [RelayCommand]
    private void NextPreviewPage()
    {
        if (PreviewCurrentPage < PreviewTotalPages && _currentPreviewPages.Count >= PreviewCurrentPage)
        {
            PreviewCurrentPage++;
            PreviewPageText = _currentPreviewPages[PreviewCurrentPage - 1];
        }
    }

    [RelayCommand]
    private void PreviousPreviewPage()
    {
        if (PreviewCurrentPage > 1 && _currentPreviewPages.Count >= PreviewCurrentPage - 1)
        {
            PreviewCurrentPage--;
            PreviewPageText = _currentPreviewPages[PreviewCurrentPage - 1];
        }
    }
    
    private async Task InitializeAsync()
    {
        IsLoading = true;
        SetStatus(() => StatusMessage = _languageService.GetString("Status_LoadingSavedFiles"));
        
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
            SetStatus(() => StatusMessage = _languageService.GetString("Status_SavedFilesLoaded", allDocuments.Count));
        }
        catch (Exception ex)
        {
            SetStatus(() => StatusMessage = _languageService.GetString("Status_Error", ex.Message));
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
        FavoriteCount = Documents.Count(d => d.IsFavorite);
        ImageCount = Documents.Count(d => d.FileExtension is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".tiff");
        PdfCount = Documents.Count(d => d.FileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase));
        WordCount = Documents.Count(d => d.FileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase) || d.FileExtension.Equals(".doc", StringComparison.OrdinalIgnoreCase));
        ExcelCount = Documents.Count(d => d.FileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) || d.FileExtension.Equals(".xls", StringComparison.OrdinalIgnoreCase));
        PptCount = Documents.Count(d => d.FileExtension.Equals(".pptx", StringComparison.OrdinalIgnoreCase) || d.FileExtension.Equals(".ppt", StringComparison.OrdinalIgnoreCase));

        IEnumerable<Document> docs = Documents;

        if (!string.IsNullOrWhiteSpace(DocumentFilterText))
        {
            var filter = DocumentFilterText.Trim().ToLower();
            docs = docs.Where(d => d.FileName.ToLower().Contains(filter) || (d.Tags != null && d.Tags.Any(t => t.ToLower().Contains(filter))));
        }

        if (SelectedFileFilter != "All")
        {
            docs = SelectedFileFilter switch
            {
                "Favorites" => docs.Where(d => d.IsFavorite),
                "Image" => docs.Where(d => d.FileExtension is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".tiff"),
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
    private void ToggleFavorite(Document? doc)
    {
        if (doc == null) return;
        doc.IsFavorite = !doc.IsFavorite;
        _documentService.SaveDocumentMetadata();
        UpdateFilteredDocuments();
    }

    [RelayCommand]
    private void AddTag(Document? doc)
    {
        if (doc == null || string.IsNullOrWhiteSpace(doc.TagInputText)) return;

        var input = doc.TagInputText.Trim().TrimStart('#');
        var newTags = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var tag in newTags)
        {
            var cleanTag = tag.Trim();
            if (!string.IsNullOrWhiteSpace(cleanTag) && !doc.Tags.Contains(cleanTag, StringComparer.OrdinalIgnoreCase))
            {
                doc.Tags.Add(cleanTag);
            }
        }

        doc.TagInputText = string.Empty;
        _documentService.SaveDocumentMetadata();
        UpdateFilteredDocuments();
    }

    [RelayCommand]
    private void RemoveTag(object? parameter)
    {
        if (parameter is ValueTuple<Document, string> tuple)
        {
            var (doc, tag) = tuple;
            if (doc != null && doc.Tags.Contains(tag))
            {
                doc.Tags.Remove(tag);
                _documentService.SaveDocumentMetadata();
                UpdateFilteredDocuments();
            }
        }
    }

    [RelayCommand]
    private async Task LoadFiles()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Desteklenen Tüm Dosyalar|*.pdf;*.xlsx;*.xls;*.docx;*.doc;*.pptx;*.ppt;*.png;*.jpg;*.jpeg;*.bmp;*.tiff|PDF Dosyaları|*.pdf|Word Dosyaları|*.docx;*.doc|Excel Dosyaları|*.xlsx;*.xls|PowerPoint Sunumları|*.pptx;*.ppt|Görsel (OCR) Dosyaları|*.png;*.jpg;*.jpeg;*.bmp;*.tiff|Tüm Dosyalar|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true)
        {
            IsLoading = true;
            SetStatus(() => StatusMessage = _languageService.GetString("Status_LoadingFiles"));

            try
            {
                var filesToLoad = dialog.FileNames
                    .Where(path => !Documents.Any(d => d.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (filesToLoad.Any())
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
                }

                SetStatus(() => StatusMessage = _languageService.GetString("Status_FilesProcessed", dialog.FileNames.Length, Documents.Count));
            }
            catch (Exception ex)
            {
                SetStatus(() => StatusMessage = _languageService.GetString("Status_Error", ex.Message));
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
        var validExtensions = new[] { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt", ".png", ".jpg", ".jpeg", ".bmp", ".tiff" };
        var filesToLoad = filePaths
            .Where(path => validExtensions.Contains(Path.GetExtension(path).ToLower()))
            .Where(path => !Documents.Any(d => d.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (!filesToLoad.Any()) return;

        IsLoading = true;
        SetStatus(() => StatusMessage = _languageService.GetString("Status_DraggingFiles"));

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
            SetStatus(() => StatusMessage = _languageService.GetString("Status_FilesAdded", filesToLoad.Count, Documents.Count));
            
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                _ = PerformSearchAsync();
            }
        }
        catch (Exception ex)
        {
            SetStatus(() => StatusMessage = _languageService.GetString("Status_Error", ex.Message));
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
        SetStatus(() => StatusMessage = _languageService.GetString("Status_FileRemoved", document.FileName));
        
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
            SetStatus(() => StatusMessage = "");
            return;
        }

        var allDocuments = _documentService.GetAllDocuments();
        
        if (allDocuments == null || !allDocuments.Any())
        {
            SearchResults.Clear();
            SetStatus(() => StatusMessage = _languageService.GetString("Status_NoFiles"));
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

        SetStatus(() => StatusMessage = _languageService.GetString("Status_ResultsFound", results.Count));
    }

}

