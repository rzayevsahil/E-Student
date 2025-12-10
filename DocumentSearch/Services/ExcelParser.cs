using DocumentSearch.Models;
using System.IO;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace DocumentSearch.Services;

public class ExcelParser : IExcelParser
{
    // Poz No pattern: tam olarak 2 rakam.3 rakam.4 rakam (15.100.1001 gibi)
    private static readonly Regex PozNoPattern = new Regex(@"^\s*(\d{2}\.\d{3}\.\d{4})\s*$", RegexOptions.Compiled);
    
    // Birim pattern: Tanım'dan çıkarılacak
    private static readonly Regex BirimPattern = new Regex(@"\b(Ton|m³|m²|Ad|1000\s+Ad|100\s+m²)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    // Fiyat pattern: Tanım'dan çıkarılacak (virgül veya nokta ile ayrılmış)
    private static readonly Regex FiyatPattern = new Regex(@"\b\d{1,3}[.,]\d{2}\b", RegexOptions.Compiled);

    public List<PriceItem> ParseExcel(string filePath)
    {
        var priceItems = new List<PriceItem>();
        var fileName = Path.GetFileName(filePath);

        try
        {
            using var workbook = new XLWorkbook(filePath);
            
            foreach (var worksheet in workbook.Worksheets)
            {
                // Worksheet boşsa atla
                if (worksheet.RowsUsed().Count() == 0)
                    continue;

                var usedRange = worksheet.RangeUsed();
                if (usedRange == null)
                    continue;

                var startRow = usedRange.FirstRow().RowNumber();
                var endRow = usedRange.LastRow().RowNumber();
                var startCol = usedRange.FirstColumn().ColumnNumber();
                var endCol = usedRange.LastColumn().ColumnNumber();

                // Header satırını bul (Poz No, Tanım, Birim, Fiyat gibi)
                int headerRow = startRow;
                int pozNoCol = -1, tanimCol = -1, birimCol = -1, fiyatCol = -1;

                // İlk 20 satırda header ara (daha geniş aralık)
                for (int row = startRow; row <= Math.Min(startRow + 20, endRow); row++)
                {
                    for (int col = startCol; col <= endCol; col++)
                    {
                        var cellValue = worksheet.Cell(row, col).GetString()?.ToLower().Trim() ?? "";
                        
                        // Poz No: "poz no", "pozno", "poz" veya "no" içerebilir
                        if (pozNoCol == -1 && (cellValue.Contains("poz no") || cellValue.Contains("pozno") || 
                            (cellValue.Contains("poz") && cellValue.Contains("no")) || 
                            cellValue == "poz no" || cellValue == "pozno"))
                            pozNoCol = col;
                        
                        // Tanım: "tanım", "tanim", "açıklama", "aciklama" içerebilir
                        if (tanimCol == -1 && (cellValue.Contains("tanım") || cellValue.Contains("tanim") || 
                            cellValue.Contains("açıklama") || cellValue.Contains("aciklama") ||
                            cellValue == "tanım" || cellValue == "tanim"))
                            tanimCol = col;
                        
                        // Birim: "birim" içerebilir
                        if (birimCol == -1 && cellValue.Contains("birim"))
                            birimCol = col;
                        
                        // Fiyat: "fiyat", "endeks", "tüik" içerebilir
                        if (fiyatCol == -1 && (cellValue.Contains("fiyat") || cellValue.Contains("endeks") || 
                            cellValue.Contains("tüik") || cellValue.Contains("tuik")))
                            fiyatCol = col;
                    }
                    
                    // Eğer en az bir kolon bulunduysa, bu satır header olabilir
                    if (pozNoCol > 0 || tanimCol > 0 || birimCol > 0 || fiyatCol > 0)
                    {
                        headerRow = row;
                        // Tüm kolonları bulmaya çalış, ama en az birini bulduysa devam et
                        if (pozNoCol > 0 && tanimCol > 0)
                            break; // İki önemli kolon bulundu, yeterli
                    }
                }

                // Eğer header bulunamadıysa, varsayılan kolonları kullan
                if (pozNoCol == -1) pozNoCol = 1;
                if (tanimCol == -1) tanimCol = 2;
                if (birimCol == -1) birimCol = 3;
                if (fiyatCol == -1) fiyatCol = 4;

                // Veri satırlarını oku
                for (int row = headerRow + 1; row <= endRow; row++)
                {
                    // Hücre değerlerini al
                    var pozNoRaw = worksheet.Cell(row, pozNoCol).GetString()?.Trim() ?? "";
                    var tanimRaw = worksheet.Cell(row, tanimCol).GetString()?.Trim() ?? "";
                    var birimRaw = worksheet.Cell(row, birimCol).GetString()?.Trim() ?? "";
                    var fiyatRaw = worksheet.Cell(row, fiyatCol).GetString()?.Trim() ?? "";
                    
                    // Eğer tüm hücreler boşsa, bu satırı atla
                    if (string.IsNullOrWhiteSpace(pozNoRaw) && 
                        string.IsNullOrWhiteSpace(tanimRaw) && 
                        string.IsNullOrWhiteSpace(birimRaw) && 
                        string.IsNullOrWhiteSpace(fiyatRaw))
                        continue;

                    // Poz No'yu temizle ve formatını kontrol et
                    string? pozNo = null;
                    if (!string.IsNullOrWhiteSpace(pozNoRaw))
                    {
                        // Poz No pattern'ine uyuyor mu kontrol et (daha esnek)
                        // Önce tam pattern'i dene
                        var pozMatch = PozNoPattern.Match(pozNoRaw);
                        if (pozMatch.Success && pozMatch.Groups.Count > 1)
                        {
                            pozNo = pozMatch.Groups[1].Value;
                        }
                        else
                        {
                            // Pattern'e uymuyorsa, içinde Poz No formatı var mı kontrol et
                            var flexiblePattern = new Regex(@"(\d{2}\.\d{3}\.\d{4})", RegexOptions.Compiled);
                            var flexMatch = flexiblePattern.Match(pozNoRaw);
                            if (flexMatch.Success && flexMatch.Groups.Count > 1)
                            {
                                pozNo = flexMatch.Groups[1].Value;
                            }
                            else
                            {
                                // Hiç pattern yoksa, sadece temizle ve kullan
                                pozNo = pozNoRaw;
                            }
                        }
                    }

                    // Tanım'ı temizle (Birim ve Fiyat bilgilerini çıkar)
                    string? tanim = null;
                    if (!string.IsNullOrWhiteSpace(tanimRaw))
                    {
                        tanim = CleanTanim(tanimRaw);
                    }

                    // Poz No veya Tanım varsa ekle
                    if (!string.IsNullOrWhiteSpace(pozNo) || !string.IsNullOrWhiteSpace(tanim))
                    {
                        priceItems.Add(new PriceItem
                        {
                            PozNo = pozNo,
                            Tanim = tanim,
                            Birim = birimRaw,
                            Fiyat = fiyatRaw,
                            DocumentPath = filePath,
                            DocumentName = fileName,
                            PageNumber = 0 // Excel'de sayfa numarası yok
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Hata durumunda exception'ı fırlat (debug için)
            throw new Exception($"Excel dosyası parse edilirken hata: {ex.Message}\nDosya: {filePath}\nİç Hata: {ex.InnerException?.Message}", ex);
        }

        return priceItems;
    }
    
    private string CleanTanim(string tanim)
    {
        if (string.IsNullOrWhiteSpace(tanim))
            return tanim;
        
        // Fiyat pattern'lerini çıkar (örneğin "254,78" gibi)
        tanim = FiyatPattern.Replace(tanim, " ").Trim();
        
        // Birim pattern'lerini çıkar, ama sayı ile birlikte olanları koru
        // Örneğin "1 ton" -> "1" kalmalı, "ton" çıkarılmalı
        var birimWithNumber = new Regex(@"(\d+)\s+(Ton|m³|m²|Ad|1000\s+Ad|100\s+m²)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        tanim = birimWithNumber.Replace(tanim, "$1"); // Sadece sayıyı bırak
        
        // Bağımsız birim kelimelerini çıkar
        tanim = BirimPattern.Replace(tanim, " ").Trim();
        
        // "TÜİK Endeksleriyle Güncel Fiyatlar" gibi ifadeleri çıkar
        tanim = Regex.Replace(tanim, @"TÜİK\s+Endeksleriyle\s+Güncel\s+Fiyatlar", " ", RegexOptions.IgnoreCase);
        tanim = Regex.Replace(tanim, @"TUIK\s+Endeksleriyle\s+Guncel\s+Fiyatlar", " ", RegexOptions.IgnoreCase);
        
        // Fazla boşlukları temizle
        tanim = Regex.Replace(tanim, @"\s+", " ").Trim();
        
        // Uzunluk kontrolü
        if (tanim.Length > 500)
        {
            tanim = tanim.Substring(0, 500) + "...";
        }
        
        return tanim;
    }

    public string ExtractText(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var fullText = new System.Text.StringBuilder();
            
            foreach (var worksheet in workbook.Worksheets)
            {
                var usedRange = worksheet.RangeUsed();
                if (usedRange == null)
                    continue;

                var endRow = usedRange.LastRow().RowNumber();
                var endCol = usedRange.LastColumn().ColumnNumber();

                for (int row = 1; row <= endRow; row++)
                {
                    var rowText = new List<string>();
                    for (int col = 1; col <= endCol; col++)
                    {
                        var cellValue = worksheet.Cell(row, col).GetString();
                        if (!string.IsNullOrWhiteSpace(cellValue))
                            rowText.Add(cellValue);
                    }
                    if (rowText.Any())
                        fullText.AppendLine(string.Join(" ", rowText));
                }
            }
            
            return fullText.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}
