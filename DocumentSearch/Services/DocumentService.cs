using DocumentSearch.Models;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace DocumentSearch.Services;

public class DocumentService : IDocumentService
{
    private readonly List<Document> _documents = new();
    private readonly IPdfParser _pdfParser;
    private readonly IExcelParser _excelParser;
    private readonly IWordParser _wordParser;
    private readonly IPowerPointParser _powerPointParser;
    private readonly IOcrService _ocrService;
    private readonly string _storagePath;
    private readonly string _tempFolder;

    public DocumentService(
        IPdfParser pdfParser, 
        IExcelParser excelParser, 
        IWordParser wordParser, 
        IPowerPointParser powerPointParser,
        IOcrService ocrService)
    {
        _pdfParser = pdfParser;
        _excelParser = excelParser;
        _wordParser = wordParser;
        _powerPointParser = powerPointParser;
        _ocrService = ocrService;
        
        // AppData/Local/E-Student klasöründe sakla
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "E-Student");
        var oldAppFolder = Path.Combine(appDataPath, "DocumentSearch");

        if (Directory.Exists(oldAppFolder) && !Directory.Exists(appFolder))
        {
            try { Directory.Move(oldAppFolder, appFolder); } catch { }
        }

        Directory.CreateDirectory(appFolder);
        _storagePath = Path.Combine(appFolder, "documents.json");
        _tempFolder = Path.Combine(appFolder, "Temp");
        Directory.CreateDirectory(_tempFolder);
    }
    
    public async Task LoadSavedDocumentsAsync()
    {
        try
        {
            if (!File.Exists(_storagePath))
                return;
                
            var json = await File.ReadAllTextAsync(_storagePath);
            var documentInfos = JsonConvert.DeserializeObject<List<DocumentInfo>>(json);
            
            if (documentInfos == null || !documentInfos.Any())
                return;
            
            var validDocs = new List<Document>();
            var docsToReParse = new List<DocumentInfo>();

            foreach (var docInfo in documentInfos)
            {
                if (string.IsNullOrEmpty(docInfo.FilePath) || !File.Exists(docInfo.FilePath))
                    continue;

                var fileInfo = new FileInfo(docInfo.FilePath);
                
                // Eğer dosya tarihi ve boyutu değişmediyse ve önbellekte metni varsa direkt önbellekten yükle
                if (!string.IsNullOrEmpty(docInfo.RawContent) &&
                    docInfo.LastWriteTime == fileInfo.LastWriteTimeUtc &&
                    docInfo.FileSize == fileInfo.Length)
                {
                    validDocs.Add(new Document
                    {
                        FilePath = docInfo.FilePath,
                        FileName = docInfo.FileName,
                        FileExtension = docInfo.FileExtension,
                        FileSize = docInfo.FileSize,
                        UploadDate = docInfo.UploadDate,
                        RawContent = docInfo.RawContent,
                        IsFavorite = docInfo.IsFavorite,
                        Tags = new ObservableCollection<string>(docInfo.Tags ?? new List<string>())
                    });
                }
                else
                {
                    // Dosya değiştirilmiş veya metni yoksa tekrar parse et
                    docsToReParse.Add(docInfo);
                }
            }

            lock (_documents)
            {
                _documents.Clear();
                _documents.AddRange(validDocs);
            }

            // Yeniden parse edilmesi gereken dosyaları paralel yükle
            if (docsToReParse.Any())
            {
                var tasks = docsToReParse.Select(async info =>
                {
                    var doc = await LoadDocumentAsync(info.FilePath);
                    doc.IsFavorite = info.IsFavorite;
                    if (info.Tags != null && info.Tags.Any())
                    {
                        doc.Tags = new ObservableCollection<string>(info.Tags);
                    }
                    return doc;
                });
                await Task.WhenAll(tasks);
            }
        }
        catch
        {
            // Hata durumunda sessizce devam et
        }
    }

    public async Task<Document> LoadDocumentAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var extension = fileInfo.Extension.ToLower();
        
        var document = new Document
        {
            FilePath = filePath,
            FileName = fileInfo.Name,
            FileExtension = extension,
            FileSize = fileInfo.Length,
            UploadDate = DateTime.Now
        };

        string rawContent;

        if (_ocrService.IsSupportedExtension(extension))
        {
            rawContent = await _ocrService.ExtractTextAsync(filePath);
        }
        else
        {
            rawContent = await Task.Run(() =>
            {
                return extension switch
                {
                    ".pdf" => _pdfParser.ExtractText(filePath),
                    ".xlsx" or ".xls" => _excelParser.ExtractText(filePath),
                    ".docx" or ".doc" => _wordParser.ExtractText(filePath),
                    ".pptx" or ".ppt" => _powerPointParser.ExtractText(filePath),
                    _ => string.Empty
                };
            });
        }

        document.RawContent = rawContent;

        lock (_documents)
        {
            var existing = _documents.FirstOrDefault(d => d.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                document.IsFavorite = existing.IsFavorite;
                document.Tags = existing.Tags;
                _documents.Remove(existing);
            }
            _documents.Add(document);
        }
        
        SaveDocuments();
        
        return document;
    }

    public void RemoveDocument(string filePath)
    {
        lock (_documents)
        {
            _documents.RemoveAll(d => d.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        }
        SaveDocuments();
    }

    public List<Document> GetAllDocuments()
    {
        lock (_documents)
        {
            return _documents.ToList();
        }
    }

    public void SaveDocumentMetadata()
    {
        SaveDocuments();
    }
    
    private void SaveDocuments()
    {
        try
        {
            List<DocumentInfo> documentInfos;
            lock (_documents)
            {
                documentInfos = _documents.Select(d =>
                {
                    DateTime lastWrite = DateTime.MinValue;
                    try
                    {
                        if (File.Exists(d.FilePath))
                            lastWrite = File.GetLastWriteTimeUtc(d.FilePath);
                    }
                    catch { }

                    return new DocumentInfo
                    {
                        FilePath = d.FilePath,
                        FileName = d.FileName,
                        FileExtension = d.FileExtension,
                        FileSize = d.FileSize,
                        UploadDate = d.UploadDate,
                        LastWriteTime = lastWrite,
                        RawContent = d.RawContent,
                        IsFavorite = d.IsFavorite,
                        Tags = d.Tags.ToList()
                    };
                }).ToList();
            }

            var json = JsonConvert.SerializeObject(documentInfos, Formatting.Indented);
            File.WriteAllText(_storagePath, json);
        }
        catch
        {
            // Hata durumunda sessizce devam et
        }
    }
    
    private class DocumentInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string RawContent { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
