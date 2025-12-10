using DocumentSearch.Models;

namespace DocumentSearch.Services;

public interface IExcelParser
{
    List<PriceItem> ParseExcel(string filePath);
    string ExtractText(string filePath);
}

