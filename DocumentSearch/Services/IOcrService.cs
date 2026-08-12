namespace DocumentSearch.Services;

public interface IOcrService
{
    Task<string> ExtractTextAsync(string imagePath);
    Task<string> ExtractTextFromBytesAsync(byte[] imageBytes);
    bool IsSupportedExtension(string extension);
}
