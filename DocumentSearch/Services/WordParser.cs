using DocumentSearch.Models;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using SyncfusionPdf = Syncfusion.Pdf;
using PdfPigDocument = UglyToad.PdfPig.PdfDocument;

namespace DocumentSearch.Services;

public class WordParser : IWordParser
{
    public List<PriceItem> ParseWord(string filePath)
    {
        var priceItems = new List<PriceItem>();
        var fileName = Path.GetFileName(filePath);

        try
        {
            using var wordDocument = WordprocessingDocument.Open(filePath, false);
            var body = wordDocument.MainDocumentPart?.Document?.Body;
            
            if (body == null)
                return priceItems;

            // Tabloları işle
            foreach (var table in body.Elements<Table>())
            {
                var rows = table.Elements<TableRow>().ToList();
                if (rows.Count < 2)
                    continue;

                // İlk satır header olabilir
                var headerRow = rows[0];
                var headerCells = headerRow.Elements<TableCell>().ToList();
                
                int pozNoCol = -1, tanimCol = -1, birimCol = -1, fiyatCol = -1;

                // Header'ı bul
                for (int i = 0; i < headerCells.Count; i++)
                {
                    var cellText = GetTextFromCell(headerCells[i]).ToLower();
                    if (cellText.Contains("poz") || cellText.Contains("no"))
                        pozNoCol = i;
                    if (cellText.Contains("tanım") || cellText.Contains("tanim") || cellText.Contains("açıklama"))
                        tanimCol = i;
                    if (cellText.Contains("birim"))
                        birimCol = i;
                    if (cellText.Contains("fiyat") || cellText.Contains("endeks"))
                        fiyatCol = i;
                }

                // Eğer header bulunamadıysa varsayılan değerler
                if (pozNoCol == -1) pozNoCol = 0;
                if (tanimCol == -1) tanimCol = 1;
                if (birimCol == -1) birimCol = 2;
                if (fiyatCol == -1) fiyatCol = 3;

                // Veri satırlarını işle
                for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
                {
                    var row = rows[rowIndex];
                    var cells = row.Elements<TableCell>().ToList();

                    if (cells.Count == 0)
                        continue;

                    var pozNo = pozNoCol < cells.Count ? GetTextFromCell(cells[pozNoCol]).Trim() : "";
                    var tanim = tanimCol < cells.Count ? GetTextFromCell(cells[tanimCol]).Trim() : "";
                    var birim = birimCol < cells.Count ? GetTextFromCell(cells[birimCol]).Trim() : "";
                    var fiyat = fiyatCol < cells.Count ? GetTextFromCell(cells[fiyatCol]).Trim() : "";

                    if (!string.IsNullOrWhiteSpace(pozNo) || !string.IsNullOrWhiteSpace(tanim))
                    {
                        priceItems.Add(new PriceItem
                        {
                            PozNo = pozNo,
                            Tanim = tanim,
                            Birim = birim,
                            Fiyat = fiyat,
                            DocumentPath = filePath,
                            DocumentName = fileName
                        });
                    }
                }
            }

            // Tablo yoksa, tüm metni al
            if (priceItems.Count == 0)
            {
                var text = ExtractText(filePath);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    priceItems.Add(new PriceItem
                    {
                        Tanim = text,
                        DocumentPath = filePath,
                        DocumentName = fileName
                    });
                }
            }
        }
        catch
        {
            var text = ExtractText(filePath);
            if (!string.IsNullOrWhiteSpace(text))
            {
                priceItems.Add(new PriceItem
                {
                    Tanim = text,
                    DocumentPath = filePath,
                    DocumentName = fileName
                });
            }
        }

        return priceItems;
    }

    public string ExtractText(string filePath)
    {
        try
        {
            // Word → PDF → Sayfa Sayfa Okuma yaklaşımı
            return ExtractWordPages(filePath);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Word dosyasını PDF'e dönüştürüp sayfa sayfa metin çıkarır
    /// </summary>
    private string ExtractWordPages(string docxPath)
    {
        string tempPdfPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(docxPath) + "_" + Guid.NewGuid().ToString("N")[..8] + ".pdf");
        
        try
        {
            // 1) Word → PDF dönüştürme
            ConvertDocxToPdf(docxPath, tempPdfPath);
            
            // 2) PDF → Sayfa Sayfa Metin
            var pages = ExtractTextByPage(tempPdfPath);
            
            // 3) Sayfa numaralarıyla birleştir
            var result = new System.Text.StringBuilder();
            foreach (var page in pages.OrderBy(p => p.Key))
            {
                result.Append($"---PAGE_{page.Key}---");
                result.Append(page.Value);
                result.Append(" ");
            }
            
            return result.ToString().Trim();
        }
        finally
        {
            // Geçici PDF dosyasını sil
            try
            {
                if (File.Exists(tempPdfPath))
                {
                    File.Delete(tempPdfPath);
                }
            }
            catch
            {
                // Silme hatası önemsiz
            }
        }
    }

    /// <summary>
    /// Word dosyasını PDF'e dönüştürür
    /// </summary>
    private void ConvertDocxToPdf(string inputPath, string outputPath)
    {
        using (var fileStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
        {
            using (WordDocument document = new WordDocument(fileStream, FormatType.Docx))
            {
                using (DocIORenderer renderer = new DocIORenderer())
                {
                    SyncfusionPdf.PdfDocument pdf = renderer.ConvertToPDF(document);
                    using (var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        pdf.Save(outputStream);
                    }
                    pdf.Close(true);
                }
            }
        }
    }

    /// <summary>
    /// PDF'den sayfa sayfa metin çıkarır
    /// </summary>
    private Dictionary<int, string> ExtractTextByPage(string pdfPath)
    {
        var result = new Dictionary<int, string>();
        
        using (PdfPigDocument document = PdfPigDocument.Open(pdfPath))
        {
            int pageCount = document.NumberOfPages;
            for (int i = 1; i <= pageCount; i++)
            {
                var page = document.GetPage(i);
                string text = page.Text;
                result.Add(i, text);
            }
        }
        
        return result;
    }


    private string ExtractTextFromBody(OpenXmlElement? body)
    {
        if (body == null)
            return string.Empty;

        var result = new System.Text.StringBuilder();
        foreach (var element in body.Elements())
        {
            if (element is Paragraph paragraph)
            {
                var text = GetTextFromParagraph(paragraph);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    result.Append(text);
                    result.Append(" ");
                }
            }
            else if (element is Table table)
            {
                result.Append(GetTextFromTable(table));
            }
        }
        return result.ToString().Trim();
    }

    private string GetTextFromParagraph(Paragraph paragraph)
    {
        var texts = new List<string>();
        
        // Tüm Text elementlerini topla (Run içindekiler dahil)
        foreach (var text in paragraph.Descendants<Text>())
        {
            if (!string.IsNullOrWhiteSpace(text.Text))
            {
                texts.Add(text.Text);
            }
        }
        
        // Eğer Text bulunamadıysa, InnerText kullan
        if (texts.Count == 0 && !string.IsNullOrWhiteSpace(paragraph.InnerText))
        {
            return paragraph.InnerText;
        }
        
        // Boşluksuz birleştir (14/07/2025 gibi tarihler için)
        return string.Concat(texts);
    }

    private string GetTextFromTable(Table table)
    {
        var result = new System.Text.StringBuilder();
        foreach (var row in table.Elements<TableRow>())
        {
            foreach (var cell in row.Elements<TableCell>())
            {
                var cellText = GetTextFromCell(cell);
                if (!string.IsNullOrWhiteSpace(cellText))
                {
                    result.Append(cellText);
                    result.Append(" ");
                }
            }
            result.AppendLine();
        }
        return result.ToString();
    }

    private string GetTextFromCell(TableCell cell)
    {
        var texts = new List<string>();
        
        // Hücre içindeki tüm paragrafları işle
        foreach (var paragraph in cell.Elements<Paragraph>())
        {
            var paragraphText = GetTextFromParagraph(paragraph);
            if (!string.IsNullOrWhiteSpace(paragraphText))
            {
                texts.Add(paragraphText);
            }
        }
        
        // Eğer paragraf yoksa, direkt Text elementlerini al
        if (texts.Count == 0)
        {
            foreach (var text in cell.Descendants<Text>())
            {
                if (!string.IsNullOrWhiteSpace(text.Text))
                {
                    texts.Add(text.Text);
                }
            }
        }
        
        // Eğer hala metin yoksa, InnerText kullan
        if (texts.Count == 0 && !string.IsNullOrWhiteSpace(cell.InnerText))
        {
            return cell.InnerText;
        }
        
        // Boşluksuz birleştir (14/07/2025 gibi tarihler için)
        return string.Concat(texts);
    }
}

