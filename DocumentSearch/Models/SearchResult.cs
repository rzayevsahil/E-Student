namespace DocumentSearch.Models;

public class SearchResult
{
    public string DocumentPath { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 0;
    public string Snippet { get; set; } = string.Empty;
    public string PageText { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
}

