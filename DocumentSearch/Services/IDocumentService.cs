using DocumentSearch.Models;

namespace DocumentSearch.Services;

public interface IDocumentService
{
    Task<Document> LoadDocumentAsync(string filePath);
    void RemoveDocument(string filePath);
    List<Document> GetAllDocuments();
    Task LoadSavedDocumentsAsync();
}

