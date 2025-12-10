using DocumentSearch.Models;
using System.IO;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace DocumentSearch.Services;

// PDF satırlarını temsil eden sınıf
internal class ParsedLine
{
    public int Page { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class PdfParser : IPdfParser
{
    // Poz No pattern: 15.100.1001 gibi format (tam olarak 2 rakam.3 rakam.4 rakam)
    // Örnek: 15.100.1001 ✓, 15.100.10011 ✗ (bu durumda 15.100.1001 Poz No, "1" tanım olarak ayrılmalı)
    // Pattern: tam olarak 2 rakam.3 rakam.4 rakam, sonrasında rakam olmamalı veya boşluk olmalı
    public static readonly Regex PozNoPattern = new Regex(@"(\d{2}\.\d{3}\.\d{4})(?!\d)", RegexOptions.Compiled);
    
    // Birim pattern: Ton, m³, m², Ad, 1000 Ad, 100 m² gibi
    private static readonly Regex BirimPattern = new Regex(@"\b(Ton|m³|m²|Ad|1000\s+Ad|100\s+m²)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    // Fiyat pattern: Sayısal değerler (virgül veya nokta ile)
    private static readonly Regex FiyatPattern = new Regex(@"\b(\d{1,3}[.,]\d{2})\b", RegexOptions.Compiled);

    public List<PriceItem> ParsePdf(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        try
        {
            // PDF'i kelime kelime oku ve satırları oluştur (tablo yapısını korur)
            var lines = ExtractPdfLines(filePath);
            
            // Satırları parse et ve PriceItem'lara dönüştür
            var priceItems = ParseLines(lines, filePath, fileName);
            
            return priceItems;
        }
        catch
        {
            // Hata durumunda boş liste döndür
            return new List<PriceItem>();
        }
    }
    
    /// <summary>
    /// PDF'i kelime kelime okuyup satırları oluşturur (tablo yapısını korur)
    /// Geliştirilmiş versiyon: Daha hassas satır algılama ve tablo yapısını koruma
    /// </summary>
    private List<ParsedLine> ExtractPdfLines(string filePath)
    {
        var lines = new List<ParsedLine>();
        
        using var document = PdfDocument.Open(filePath);
        
        foreach (var page in document.GetPages())
        {
            var words = page.GetWords().ToList();
            
            if (words.Count == 0)
                continue;
            
            // Kelimeleri Y koordinatına göre grupla (aynı satır = benzer Y koordinatı)
            // Daha hassas gruplama için tolerance kullan (satırlar arası boşluk farklı olabilir)
            var yCoordinates = words.Select(w => w.BoundingBox.Bottom).Distinct().OrderByDescending(y => y).ToList();
            
            // Satırları belirle (yakın Y koordinatlarını birleştir)
            var lineGroups = new List<List<UglyToad.PdfPig.Content.Word>>();
            double tolerance = 2.0; // Satırlar arası tolerans (piksel)
            
            foreach (var word in words)
            {
                bool added = false;
                foreach (var group in lineGroups)
                {
                    // Bu kelime mevcut bir gruba ait mi? (Y koordinatı yakınsa)
                    if (group.Count > 0)
                    {
                        var groupY = group[0].BoundingBox.Bottom;
                        if (Math.Abs(word.BoundingBox.Bottom - groupY) <= tolerance)
                        {
                            group.Add(word);
                            added = true;
                            break;
                        }
                    }
                }
                
                if (!added)
                {
                    // Yeni satır grubu oluştur
                    lineGroups.Add(new List<UglyToad.PdfPig.Content.Word> { word });
                }
            }
            
            // Her satır grubunu işle
            foreach (var group in lineGroups.OrderByDescending(g => g[0].BoundingBox.Bottom))
            {
                // Aynı satırdaki kelimeleri soldan sağa sırala (X koordinatına göre)
                var sortedWords = group.OrderBy(w => w.BoundingBox.Left).ToList();
                
                // Kelimeleri birleştir (boşluk ekle)
                var lineText = new System.Text.StringBuilder();
                for (int i = 0; i < sortedWords.Count; i++)
                {
                    if (i > 0)
                    {
                        // Önceki kelime ile bu kelime arasındaki mesafeyi kontrol et
                        var prevRight = sortedWords[i - 1].BoundingBox.Right;
                        var currentLeft = sortedWords[i].BoundingBox.Left;
                        var gap = currentLeft - prevRight;
                        
                        // Eğer gap çok büyükse (örneğin 10 piksel), muhtemelen farklı sütunlar
                        // Bu durumda tab karakteri ekle (sütun ayırıcı)
                        if (gap > 10)
                        {
                            lineText.Append("\t");
                        }
                        else
                        {
                            lineText.Append(" ");
                        }
                    }
                    lineText.Append(sortedWords[i].Text);
                }
                
                var finalLineText = lineText.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(finalLineText))
                {
                    lines.Add(new ParsedLine
                    {
                        Page = page.Number,
                        Text = finalLineText
                    });
                }
            }
        }
        
        return lines;
    }
    
    /// <summary>
    /// Satırları parse edip PriceItem'lara dönüştürür (blok bazlı parse - tanım çok satırlı olabilir)
    /// </summary>
    private List<PriceItem> ParseLines(List<ParsedLine> lines, string filePath, string fileName)
    {
        var result = new List<PriceItem>();
        var descriptionBuffer = new List<string>(); // Tanım satırlarını biriktirmek için
        
        foreach (var line in lines)
        {
            var match = PozNoPattern.Match(line.Text);
            
            if (!match.Success)
            {
                // Bu satır Poz No içermiyor → tanımın devamıdır, buffer'a ekle
                var trimmedLine = line.Text.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    descriptionBuffer.Add(trimmedLine);
                }
                continue;
            }
            
            // Buraya geldiysek bu satır Poz satırıdır
            string pozNo = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value.Trim();
            
            if (string.IsNullOrWhiteSpace(pozNo))
            {
                // Poz No boşsa, bu satırı da buffer'a ekle
                var trimmedLine = line.Text.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    descriptionBuffer.Add(trimmedLine);
                }
                continue;
            }
            
            // Poz No'dan sonraki metni al
            string afterPoz = line.Text.Substring(match.Index + match.Length).Trim();
            
            // Birim'i bul
            var birimMatch = BirimPattern.Match(afterPoz);
            string birim = birimMatch.Success ? birimMatch.Value.Trim() : "";
            
            // Fiyat'ı bul (Birim'den sonra)
            string fiyat = "";
            if (birimMatch.Success)
            {
                var afterBirim = afterPoz.Substring(birimMatch.Index + birimMatch.Length).Trim();
                var priceMatch = FiyatPattern.Match(afterBirim);
                if (priceMatch.Success)
                {
                    fiyat = priceMatch.Value.Trim();
                }
            }
            else
            {
                // Birim yoksa, direkt içerikte fiyat ara
                var priceMatch = FiyatPattern.Match(afterPoz);
                if (priceMatch.Success)
                {
                    fiyat = priceMatch.Value.Trim();
                }
            }
            
            // Tanım: Poz satırından önce gelen tüm satırlar (buffer'daki satırlar)
            string tanim = string.Join(" ", descriptionBuffer).Trim();
            
            // Tanım'ı temizle
            tanim = CleanTanim(tanim, pozNo);
            
            result.Add(new PriceItem
            {
                PozNo = pozNo,
                Tanim = tanim,
                Birim = birim,
                Fiyat = fiyat,
                DocumentPath = filePath,
                DocumentName = fileName,
                PageNumber = line.Page
            });
            
            // Yeni poz için buffer'ı temizle
            descriptionBuffer.Clear();
        }
        
        return result;
    }
    
    /// <summary>
    /// Tanım'dan gereksiz bilgileri temizler
    /// </summary>
    private string CleanTanim(string tanim, string pozNo)
    {
        if (string.IsNullOrWhiteSpace(tanim))
            return tanim;
        
        // Poz No'yu çıkar (eğer içeride geçiyorsa)
        tanim = tanim.Replace(pozNo, "").Trim();
        
        // Fiyat pattern'lerini çıkar (örneğin "254,78" gibi)
        var fiyatPatternInTanim = new Regex(@"\b\d{1,3}[.,]\d{2}\b", RegexOptions.Compiled);
        tanim = fiyatPatternInTanim.Replace(tanim, " ").Trim();
        
        // Birim pattern'lerini çıkar, ama sayı ile birlikte olanları koru
        // Örneğin "1 ton" -> "1" kalmalı
        var birimWithNumber = new Regex(@"(\d+)\s+(Ton|m³|m²|Ad|1000\s+Ad|100\s+m²)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        tanim = birimWithNumber.Replace(tanim, "$1").Trim();
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
            using var document = PdfDocument.Open(filePath);
            var fullText = new System.Text.StringBuilder();
            
            foreach (var page in document.GetPages())
            {
                // Her sayfayı ayırmak için sayfa ayırıcı ekle
                fullText.AppendLine($"---PAGE_{page.Number}---");
                fullText.AppendLine(page.Text);
            }
            
            return fullText.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}
