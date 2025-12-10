using DocumentSearch.Models;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

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
            using var wordDocument = WordprocessingDocument.Open(filePath, false);
            var body = wordDocument.MainDocumentPart?.Document?.Body;
            
            if (body == null)
                return string.Empty;

            return body.InnerText;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string GetTextFromCell(TableCell cell)
    {
        return string.Join(" ", cell.Descendants<Text>().Select(t => t.Text));
    }
}

