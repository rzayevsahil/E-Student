using System;
using System.IO;

namespace DocumentSearch.Services;

/// <summary>
/// Kendi patch sistemimiz - Binary diff/patch uygulama
/// </summary>
public static class MyPatchService
{
    private const string MagicNumber = "MYPT"; // My Patch
    
    /// <summary>
    /// Patch dosyasını uygular ve yeni exe oluşturur
    /// </summary>
    /// <param name="oldExePath">Eski exe dosyası yolu</param>
    /// <param name="patchFilePath">Patch dosyası yolu</param>
    /// <param name="newExePath">Yeni exe dosyası yolu</param>
    public static void ApplyPatch(string oldExePath, string patchFilePath, string newExePath)
    {
        if (!File.Exists(oldExePath))
            throw new FileNotFoundException("Eski exe dosyası bulunamadı.", oldExePath);
        
        if (!File.Exists(patchFilePath))
            throw new FileNotFoundException("Patch dosyası bulunamadı.", patchFilePath);
        
        // Eski exe'yi oku
        byte[] oldBytes = File.ReadAllBytes(oldExePath);
        
        // Patch dosyasını oku ve uygula
        using (var patchStream = File.OpenRead(patchFilePath))
        using (var reader = new BinaryReader(patchStream))
        {
            // Header oku
            var magic = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (magic != MagicNumber)
                throw new InvalidDataException("Geçersiz patch dosyası formatı.");
            
            int oldFileSize = reader.ReadInt32();
            int newFileSize = reader.ReadInt32();
            
            // Dosya boyutlarını kontrol et
            if (oldBytes.Length != oldFileSize)
                throw new InvalidDataException($"Eski exe boyutu uyuşmuyor. Beklenen: {oldFileSize}, Bulunan: {oldBytes.Length}");
            
            // Değişiklik sayısı
            int changeCount = reader.ReadInt32();
            
            // Yeni dosyayı oluştur (MemoryStream kullanarak dinamik boyutlandırma)
            using (var newStream = new MemoryStream(newFileSize))
            {
                int currentPos = 0;
                
                // Her değişikliği uygula
                for (int i = 0; i < changeCount; i++)
                {
                    int offset = reader.ReadInt32();
                    int oldLength = reader.ReadInt32();
                    int newLength = reader.ReadInt32();
                    
                    // Offset'e kadar olan kısmı kopyala (değişmemiş)
                    if (offset > currentPos)
                    {
                        int copyLength = offset - currentPos;
                        newStream.Write(oldBytes, currentPos, copyLength);
                        currentPos = offset;
                    }
                    
                    // Eski kısmı atla
                    currentPos += oldLength;
                    
                    // Yeni data'yı oku ve yaz
                    if (newLength > 0)
                    {
                        byte[] newData = reader.ReadBytes(newLength);
                        newStream.Write(newData, 0, newLength);
                    }
                }
                
                // Kalan kısmı kopyala
                if (currentPos < oldBytes.Length)
                {
                    int remaining = oldBytes.Length - currentPos;
                    newStream.Write(oldBytes, currentPos, remaining);
                }
                
                // Yeni exe'yi kaydet
                File.WriteAllBytes(newExePath, newStream.ToArray());
            }
            
            // Yeni exe'yi kaydet
            File.WriteAllBytes(newExePath, newBytes);
        }
    }
}

