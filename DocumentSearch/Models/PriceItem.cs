namespace DocumentSearch.Models;

public class PriceItem
{
    public string? PozNo { get; set; }
    public string? Tanim { get; set; }
    public string? Birim { get; set; }
    public string? Fiyat { get; set; }
    
    public string DocumentPath { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 0;
}

