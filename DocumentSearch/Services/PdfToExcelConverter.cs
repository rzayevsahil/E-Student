using System.IO;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using UglyToad.PdfPig;

namespace DocumentSearch.Services;

public class PdfToExcelConverter : IPdfToExcelConverter
{
    // Poz No pattern: tam olarak 2 rakam.3 rakam.4 rakam
    private static readonly Regex PozNoPattern = new Regex(@"(\d{2}\.\d{3}\.\d{4})(?!\d)", RegexOptions.Compiled);
    
    // Birim pattern
    private static readonly Regex BirimPattern = new Regex(@"\b(Ton|m³|m²|Ad|1000\s+Ad|100\s+m²)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    // Fiyat pattern
    private static readonly Regex FiyatPattern = new Regex(@"\b(\d{1,3}[.,]\d{2})\b", RegexOptions.Compiled);

    public string ConvertPdfToExcel(string pdfFilePath)
    {
        // Geçici Excel dosyası oluştur
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "DocumentSearch", "Temp");
        Directory.CreateDirectory(appFolder);
        
        var pdfFileName = Path.GetFileNameWithoutExtension(pdfFilePath);
        var excelFilePath = Path.Combine(appFolder, $"{pdfFileName}_{Guid.NewGuid():N}.xlsx");
        
        try
        {
            using var document = PdfDocument.Open(pdfFilePath);
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");
            
            // Header satırı
            worksheet.Cell(1, 1).Value = "Poz No";
            worksheet.Cell(1, 2).Value = "Tanım";
            worksheet.Cell(1, 3).Value = "Birim";
            worksheet.Cell(1, 4).Value = "Fiyat";
            
            // Header stil
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            var pageTexts = new List<(int pageNumber, string text)>();
            int pageNumber = 1;
            
            foreach (var page in document.GetPages())
            {
                pageTexts.Add((pageNumber, page.Text));
                pageNumber++;
            }
            
            // Sayfa sayfa, satır satır parse et (tablo yapısını daha iyi yakalamak için)
            int row = 2; // Header'dan sonra başla
            var processedPozNos = new HashSet<string>(); // Duplicate kontrolü için
            
            foreach (var (pageNum, pageText) in pageTexts)
            {
                // Satırlara böl (tablo yapısını yakalamak için)
                var lines = pageText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.Length < 5)
                        continue;
                    
                    // Satırda Poz No'ları bul (birden fazla olabilir)
                    var pozMatches = PozNoPattern.Matches(trimmedLine);
                    var matchList = pozMatches.Cast<Match>().ToList();
                    
                    if (matchList.Count == 0)
                        continue;
                    
                    // Her Poz No için işle
                    for (int i = 0; i < matchList.Count; i++)
                    {
                        var pozMatch = matchList[i];
                        var pozNo = pozMatch.Groups.Count > 1 ? pozMatch.Groups[1].Value : pozMatch.Value.Trim();
                        
                        if (string.IsNullOrWhiteSpace(pozNo))
                            continue;
                        
                        // Duplicate kontrolü (aynı Poz No aynı sayfada birden fazla görünmemeli)
                        var key = $"{pozNo}_{pageNum}";
                        if (processedPozNos.Contains(key))
                            continue;
                        
                        // Bu Poz No'dan sonraki içeriği al
                        int startIndex = pozMatch.Index + pozMatch.Length;
                        int endIndex = trimmedLine.Length;
                        
                        // Aynı satırdaki bir sonraki Poz No'yu bul
                        if (i < matchList.Count - 1)
                        {
                            endIndex = matchList[i + 1].Index;
                        }
                        else
                        {
                            // Son Poz No ise, satır sonuna kadar veya bir sonraki satıra kadar
                            // Bir sonraki satırda da Poz No var mı kontrol et
                            endIndex = trimmedLine.Length;
                        }
                        
                        if (endIndex > startIndex && endIndex <= trimmedLine.Length)
                        {
                            var content = trimmedLine.Substring(startIndex, endIndex - startIndex).Trim();
                            
                            // Eğer içerik çok kısaysa, bir sonraki satırı da ekle (çok satırlı tanımlar için)
                            if (content.Length < 20 && row > 2)
                            {
                                // Bir önceki satırın devamı olabilir, şimdilik mevcut içeriği kullan
                            }
                            
                            // Tanım, Birim ve Fiyat'ı çıkar
                            var (tanim, birim, fiyat) = ExtractFields(content, pozNo);
                            
                            // Excel'e yaz (Poz No veya Tanım varsa)
                            if (!string.IsNullOrWhiteSpace(pozNo) || !string.IsNullOrWhiteSpace(tanim))
                            {
                                worksheet.Cell(row, 1).Value = pozNo;
                                worksheet.Cell(row, 2).Value = tanim;
                                worksheet.Cell(row, 3).Value = birim;
                                worksheet.Cell(row, 4).Value = fiyat;
                                
                                processedPozNos.Add(key);
                                row++;
                            }
                        }
                    }
                }
            }
            
            // Eğer hala çok az veri varsa, alternatif yöntem: Tüm metni birleştirip parse et
            if (row < 100) // Çok az veri bulunduysa
            {
                var fullText = new System.Text.StringBuilder();
                foreach (var (_, text) in pageTexts)
                {
                    fullText.AppendLine(text);
                }
                var allText = fullText.ToString();
                
                // Poz No'ları bul
                var pozNoMatches = PozNoPattern.Matches(allText);
                var matchList = pozNoMatches.Cast<Match>().ToList();
                
                var additionalPozNos = new HashSet<string>();
                
                for (int i = 0; i < matchList.Count; i++)
                {
                    var match = matchList[i];
                    var pozNo = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value.Trim();
                    
                    if (string.IsNullOrWhiteSpace(pozNo))
                        continue;
                    
                    // Zaten işlenmiş mi kontrol et
                    if (processedPozNos.Contains(pozNo) || additionalPozNos.Contains(pozNo))
                        continue;
                    
                    // Bu Poz No'dan sonraki içeriği al
                    int startIndex = match.Index + match.Length;
                    int endIndex = allText.Length;
                    
                    // Bir sonraki Poz No'yu bul
                    if (i < matchList.Count - 1)
                    {
                        endIndex = matchList[i + 1].Index;
                    }
                    
                    if (endIndex > startIndex && endIndex <= allText.Length)
                    {
                        var content = allText.Substring(startIndex, Math.Min(endIndex - startIndex, 2000)).Trim();
                        
                        // Tanım, Birim ve Fiyat'ı çıkar
                        var (tanim, birim, fiyat) = ExtractFields(content, pozNo);
                        
                        // Excel'e yaz
                        if (!string.IsNullOrWhiteSpace(pozNo) || !string.IsNullOrWhiteSpace(tanim))
                        {
                            worksheet.Cell(row, 1).Value = pozNo;
                            worksheet.Cell(row, 2).Value = tanim;
                            worksheet.Cell(row, 3).Value = birim;
                            worksheet.Cell(row, 4).Value = fiyat;
                            
                            additionalPozNos.Add(pozNo);
                            row++;
                        }
                    }
                }
            }
            
            // Kolon genişliklerini ayarla
            worksheet.Column(1).Width = 15; // Poz No
            worksheet.Column(2).Width = 80; // Tanım
            worksheet.Column(3).Width = 10; // Birim
            worksheet.Column(4).Width = 15; // Fiyat
            
            workbook.SaveAs(excelFilePath);
            
            return excelFilePath;
        }
        catch
        {
            // Hata durumunda geçici dosyayı sil
            if (File.Exists(excelFilePath))
            {
                try { File.Delete(excelFilePath); } catch { }
            }
            throw;
        }
    }
    
    private (string tanim, string birim, string fiyat) ExtractFields(string content, string pozNo)
    {
        string tanim = "";
        string birim = "";
        string fiyat = "";
        
        if (string.IsNullOrWhiteSpace(content))
            return (tanim, birim, fiyat);
        
        // Birim'i bul (daha esnek pattern)
        var birimMatch = BirimPattern.Match(content);
        if (birimMatch.Success)
        {
            birim = birimMatch.Value.Trim();
        }
        
        // Fiyat'ı bul (daha esnek pattern - virgül veya nokta ile ayrılmış sayılar)
        // Önce Birim'den sonraki fiyatı ara
        var fiyatMatch = FiyatPattern.Match(content);
        if (fiyatMatch.Success)
        {
            fiyat = fiyatMatch.Value.Trim();
        }
        
        // Tanım: Poz No'dan Birim veya Fiyat'a kadar (hangisi önce geliyorsa)
        int tanimEnd = content.Length;
        
        if (birimMatch.Success && fiyatMatch.Success)
        {
            // Hem Birim hem Fiyat varsa, hangisi önce geliyorsa onu kullan
            tanimEnd = Math.Min(birimMatch.Index, fiyatMatch.Index);
        }
        else if (birimMatch.Success)
        {
            tanimEnd = birimMatch.Index;
        }
        else if (fiyatMatch.Success)
        {
            tanimEnd = fiyatMatch.Index;
        }
        
        // Tanım'ı çıkar
        if (tanimEnd > 0 && tanimEnd <= content.Length)
        {
            tanim = content.Substring(0, tanimEnd).Trim();
            // Poz No'yu çıkar (eğer içeride geçiyorsa)
            tanim = tanim.Replace(pozNo, "").Trim();
            
            // Tanım'dan fiyat pattern'lerini çıkar (örneğin "254,78" gibi)
            var fiyatPatternInTanim = new Regex(@"\b\d{1,3}[.,]\d{2}\b", RegexOptions.Compiled);
            tanim = fiyatPatternInTanim.Replace(tanim, " ").Trim();
            
            // Birim pattern'lerini çıkar, ama sayı ile birlikte olanları koru
            // Örneğin "1 ton" -> "1" kalmalı
            var birimWithNumber = new Regex(@"(\d+)\s+(Ton|m³|m²|Ad|1000\s+Ad|100\s+m²)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            tanim = birimWithNumber.Replace(tanim, "$1").Trim();
            tanim = BirimPattern.Replace(tanim, " ").Trim();
            
            // TÜİK ifadelerini çıkar
            tanim = Regex.Replace(tanim, @"TÜİK\s+Endeksleriyle\s+Güncel\s+Fiyatlar", " ", RegexOptions.IgnoreCase);
            tanim = Regex.Replace(tanim, @"TUIK\s+Endeksleriyle\s+Guncel\s+Fiyatlar", " ", RegexOptions.IgnoreCase);
            
            // Fazla boşlukları temizle
            tanim = Regex.Replace(tanim, @"\s+", " ").Trim();
            
            // Uzunluk kontrolü
            if (tanim.Length > 500)
            {
                tanim = tanim.Substring(0, 500) + "...";
            }
        }
        else
        {
            // Hiçbir şey bulunamadıysa, tüm içeriği tanım olarak al (temizleyerek)
            tanim = content.Replace(pozNo, "").Trim();
            
            // Fiyat ve birim pattern'lerini çıkar
            var fiyatPatternInTanim = new Regex(@"\b\d{1,3}[.,]\d{2}\b", RegexOptions.Compiled);
            tanim = fiyatPatternInTanim.Replace(tanim, " ").Trim();
            
            var birimWithNumber = new Regex(@"(\d+)\s+(Ton|m³|m²|Ad|1000\s+Ad|100\s+m²)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            tanim = birimWithNumber.Replace(tanim, "$1").Trim();
            tanim = BirimPattern.Replace(tanim, " ").Trim();
            tanim = Regex.Replace(tanim, @"\s+", " ").Trim();
            
            if (tanim.Length > 500)
            {
                tanim = tanim.Substring(0, 500) + "...";
            }
        }
        
        return (tanim, birim, fiyat);
    }
}
