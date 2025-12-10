namespace DocumentSearch.Models;

public class Document
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.Now;
    public List<PriceItem> PriceItems { get; set; } = new();
    public string RawContent { get; set; } = string.Empty;
}

