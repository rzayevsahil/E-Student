using DocumentSearch.Models;
using System.Text.RegularExpressions;

namespace DocumentSearch.Services;

public class SearchService : ISearchService
{
    public List<SearchResult> Search(string query, List<Document> documents)
    {
        if (string.IsNullOrWhiteSpace(query) || documents == null || !documents.Any())
            return new List<SearchResult>();

        var results = new List<SearchResult>();
        var normalizedQuery = NormalizeTurkish(query.ToLower().Trim());
        var trimmedQuery = query.Trim();

        foreach (var document in documents)
        {
            // Dosya isminde arama
            var normalizedFileName = NormalizeTurkish(document.FileName.ToLower());
            if (normalizedFileName.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                results.Add(new SearchResult
                {
                    DocumentPath = document.FilePath,
                    DocumentName = document.FileName,
                    PageNumber = 0 // Dosya adında eşleşme, sayfa yok
                });
            }

            // İçerikte sayfa bazlı arama (sadece PDF için)
            if (document.FileExtension.ToLower() == ".pdf" && !string.IsNullOrWhiteSpace(document.RawContent))
            {
                // PDF içeriğini sayfalara böl (---PAGE_X--- ayırıcısına göre)
                var pageSeparator = "---PAGE_";
                var pages = document.RawContent.Split(new[] { pageSeparator }, StringSplitOptions.RemoveEmptyEntries);
                
                for (int pageIndex = 0; pageIndex < pages.Length; pageIndex++)
                {
                    var pageContent = pages[pageIndex];
                    // Sayfa numarasını çıkar (---PAGE_1--- formatından)
                    var pageNumberMatch = System.Text.RegularExpressions.Regex.Match(pageContent, @"^(\d+)---");
                    int pageNumber = pageIndex + 1;
                    if (pageNumberMatch.Success)
                    {
                        if (int.TryParse(pageNumberMatch.Groups[1].Value, out int parsedPage))
                        {
                            pageNumber = parsedPage;
                        }
                        // Sayfa numarasını içerikten çıkar
                        pageContent = pageContent.Substring(pageNumberMatch.Length);
                    }
                    
                    var normalizedPageContent = NormalizeTurkish(pageContent.ToLower());
                    
                    // Sayfa içeriğinde arama
                    if (normalizedPageContent.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        pageContent.IndexOf(trimmedQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        results.Add(new SearchResult
                        {
                            DocumentPath = document.FilePath,
                            DocumentName = document.FileName,
                            PageNumber = pageNumber
                        });
                    }
                }
            }
            else
            {
                // PDF dışı dosyalar için (Excel, Word) - tüm içerikte ara
                if (!string.IsNullOrWhiteSpace(document.RawContent))
                {
                    var normalizedContent = NormalizeTurkish(document.RawContent.ToLower());
                    if (normalizedContent.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        document.RawContent.IndexOf(trimmedQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        results.Add(new SearchResult
                        {
                            DocumentPath = document.FilePath,
                            DocumentName = document.FileName,
                            PageNumber = 0 // Excel/Word'de sayfa yok
                        });
                    }
                }
            }
        }

        // Tekrar eden sonuçları kaldır (aynı dosya ve sayfa)
        return results
            .GroupBy(r => new { r.DocumentPath, r.PageNumber })
            .Select(g => g.First())
            .OrderBy(r => r.DocumentName)
            .ThenBy(r => r.PageNumber)
            .ToList();
    }

    private string NormalizeTurkish(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("ı", "i")
            .Replace("İ", "i")
            .Replace("ş", "s")
            .Replace("Ş", "s")
            .Replace("ğ", "g")
            .Replace("Ğ", "g")
            .Replace("ü", "u")
            .Replace("Ü", "u")
            .Replace("ö", "o")
            .Replace("Ö", "o")
            .Replace("ç", "c")
            .Replace("Ç", "c");
    }
}
