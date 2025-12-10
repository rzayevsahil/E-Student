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
    private readonly IPdfToExcelConverter _pdfToExcelConverter;
    private readonly string _storagePath;
    private readonly string _tempFolder;

    public DocumentService(IPdfParser pdfParser, IExcelParser excelParser, IWordParser wordParser, IPdfToExcelConverter pdfToExcelConverter)
    {
        _pdfParser = pdfParser;
        _excelParser = excelParser;
        _wordParser = wordParser;
        _pdfToExcelConverter = pdfToExcelConverter;
        
        // AppData/Local/DocumentSearch klasöründe sakla
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "DocumentSearch");
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
                
            var json = File.ReadAllText(_storagePath);
            var documentInfos = JsonConvert.DeserializeObject<List<DocumentInfo>>(json);
            
            if (documentInfos == null)
                return;
            
            // Kayıtlı dosyaları yükle (sadece dosya yolu geçerliyse)
            foreach (var docInfo in documentInfos)
            {
                if (string.IsNullOrEmpty(docInfo.FilePath) || !File.Exists(docInfo.FilePath))
                    continue;
                
                // Dosyayı tekrar parse et
                await LoadDocumentAsync(docInfo.FilePath);
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
                    // PDF'den sadece metin çıkar (parse etme)
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
                default:
                    rawContent = string.Empty;
                    break;
            }

            document.RawContent = rawContent;

            // Eğer dosya zaten yüklenmişse, eski halini kaldır
            _documents.RemoveAll(d => d.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            _documents.Add(document);
            
            // Dosya listesini kaydet
            SaveDocuments();
            
            return document;
        });
    }

    public void RemoveDocument(string filePath)
    {
        _documents.RemoveAll(d => d.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        SaveDocuments();
    }

    public List<Document> GetAllDocuments()
    {
        return _documents.ToList();
    }
    
    private void SaveDocuments()
    {
        try
        {
            // Sadece dosya bilgilerini kaydet
            var documentInfos = _documents.Select(d => new DocumentInfo
            {
                FilePath = d.FilePath,
                FileName = d.FileName,
                FileExtension = d.FileExtension,
                FileSize = d.FileSize,
                UploadDate = d.UploadDate
            }).ToList();
            
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
    }
}

