using DocumentSearch.Models;

namespace DocumentSearch.Services;

public interface IWordParser
{
    List<PriceItem> ParseWord(string filePath);
    string ExtractText(string filePath);
}

