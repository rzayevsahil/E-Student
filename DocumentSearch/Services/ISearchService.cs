using DocumentSearch.Models;

namespace DocumentSearch.Services;

public interface ISearchService
{
    List<SearchResult> Search(string query, List<Document> documents);
}

