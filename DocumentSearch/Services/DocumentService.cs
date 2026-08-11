using DocumentSearch.Models;
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
    private readonly IPdfToExcelConverter _pdfToExcelConverter;
    private readonly string _storagePath;
    private readonly string _tempFolder;

    public DocumentService(IPdfParser pdfParser, IExcelParser excelParser, IWordParser wordParser, IPowerPointParser powerPointParser, IPdfToExcelConverter pdfToExcelConverter)
    {
        _pdfParser = pdfParser;
        _excelParser = excelParser;
        _wordParser = wordParser;
        _powerPointParser = powerPointParser;
        _pdfToExcelConverter = pdfToExcelConverter;
        
        // AppData/Local/E-Student klasöründe sakla (Eski DocumentSearch klasörünü otomatik taşı)
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
                        RawContent = docInfo.RawContent
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
                var tasks = docsToReParse.Select(info => LoadDocumentAsync(info.FilePath));
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
        return await Task.Run(() =>
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

            switch (extension)
            {
                case ".pdf":
                    rawContent = _pdfParser.ExtractText(filePath);
                    break;
                case ".xlsx":
                case ".xls":
                    rawContent = _excelParser.ExtractText(filePath);
                    break;
                case ".docx":
                case ".doc":
                    rawContent = _wordParser.ExtractText(filePath);
                    break;
                case ".pptx":
                case ".ppt":
                    rawContent = _powerPointParser.ExtractText(filePath);
                    break;
                default:
                    rawContent = string.Empty;
                    break;
            }

            document.RawContent = rawContent;

            lock (_documents)
            {
                _documents.RemoveAll(d => d.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
                _documents.Add(document);
            }
            
            SaveDocuments();
            
            return document;
        });
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
                        RawContent = d.RawContent
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
    }
}

