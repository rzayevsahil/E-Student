using System.IO;
using System.Text;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DocumentSearch.Services;

public class OcrService : IOcrService
{
    private static readonly string[] SupportedExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".tiff" };

    public bool IsSupportedExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return false;
        return SupportedExtensions.Contains(extension.ToLowerInvariant());
    }

    public async Task<string> ExtractTextAsync(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            return string.Empty;

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(imagePath);
            using IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            // OcrEngine Bgra8 ve Premultiplied formatı gerektirir
            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            // Türkçe, İngilizce veya varsayılan sistem dili ile OCR engine dene
            OcrEngine? ocrEngine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("tr"))
                                  ?? OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"))
                                  ?? OcrEngine.TryCreateFromUserProfileLanguages();

            if (ocrEngine == null)
            {
                return string.Empty;
            }

            var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);
            if (ocrResult == null || string.IsNullOrWhiteSpace(ocrResult.Text))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.AppendLine("---PAGE_1---");
            foreach (var line in ocrResult.Lines)
            {
                sb.AppendLine(line.Text);
            }

            return sb.ToString().Trim();
        }
        catch (Exception)
        {
            // Herhangi bir WinRT / Bitmap çözme hatasında güvenli geri dönüş
            return string.Empty;
        }
    }
}
