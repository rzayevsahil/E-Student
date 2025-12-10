using DocumentSearch.Models;

namespace DocumentSearch.Services;

public interface IPdfParser
{
    List<PriceItem> ParsePdf(string filePath);
    string ExtractText(string filePath);
}

