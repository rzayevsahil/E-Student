using DocumentSearch.Models;
using System.Text.RegularExpressions;

namespace DocumentSearch.Services;

public class SearchService : ISearchService
{
    public List<SearchResult> Search(string query, List<Document> documents)
    {
        if (string.IsNullOrWhiteSpace(query) || documents == null || !documents.Any())
            return new List<SearchResult>();

        var results = new System.Collections.Concurrent.ConcurrentBag<SearchResult>();
        var normalizedQuery = NormalizeTurkish(query.ToLower().Trim());
        var trimmedQuery = query.Trim();

        Parallel.ForEach(documents, document =>
        {
            // İçerikte sayfa/slayt bazlı arama (PDF, Word ve PowerPoint için)
            var ext = document.FileExtension.ToLower();
            if ((ext == ".pdf" || ext == ".docx" || ext == ".doc" || ext == ".pptx" || ext == ".ppt") 
                && !string.IsNullOrWhiteSpace(document.RawContent))
            {
                // İçeriği sayfalara böl (---PAGE_X--- ayırıcısına göre)
                var pageSeparator = "---PAGE_";
                var pages = document.RawContent.Split(new[] { pageSeparator }, StringSplitOptions.RemoveEmptyEntries);
                
                for (int pageIndex = 0; pageIndex < pages.Length; pageIndex++)
                {
                    var pageContent = pages[pageIndex];
                    // Sayfa numarasını çıkar (---PAGE_1--- formatından)
                    var pageNumberMatch = Regex.Match(pageContent, @"^(\d+)---");
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
                            PageNumber = pageNumber,
                            FileExtension = document.FileExtension,
                            Snippet = CreateSnippet(pageContent, query),
                            PageText = pageContent.Trim()
                        });
                    }
                }
            }
            else
            {
                // Excel gibi diğer dosyalar için - tüm içerikte ara
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
                            PageNumber = 0,
                            FileExtension = document.FileExtension,
                            Snippet = CreateSnippet(document.RawContent, query),
                            PageText = document.RawContent.Trim()
                        });
                    }
                }
            }
        });

        // Tekrar eden sonuçları kaldır (aynı dosya ve sayfa)
        return results
            .GroupBy(r => new { r.DocumentPath, r.PageNumber })
            .Select(g => g.First())
            .OrderBy(r => r.DocumentName)
            .ThenBy(r => r.PageNumber)
            .ToList();
    }

    private string CreateSnippet(string content, string query)
    {
        if (string.IsNullOrWhiteSpace(content)) return string.Empty;
        if (string.IsNullOrWhiteSpace(query)) return content.Length > 200 ? content.Substring(0, 200) + "..." : content;

        var normalizedContent = NormalizeTurkish(content.ToLower());
        var normalizedQuery = NormalizeTurkish(query.ToLower().Trim());

        int index = normalizedContent.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return content.Length > 200 ? content.Substring(0, 200) + "..." : content;
        }

        int start = Math.Max(0, index - 50);
        int length = Math.Min(content.Length - start, query.Length + 120);

        string snippet = content.Substring(start, length).Replace("\r", " ").Replace("\n", " ");
        if (start > 0) snippet = "..." + snippet;
        if (start + length < content.Length) snippet += "...";

        return snippet;
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
