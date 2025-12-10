namespace DocumentSearch.Models;

public class SearchResult
{
    public string DocumentPath { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 0;
}

