namespace DocumentSearch.Services;

public interface IOcrService
{
    Task<string> ExtractTextAsync(string imagePath);
    bool IsSupportedExtension(string extension);
}
