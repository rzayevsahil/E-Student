# DocumentSearch - Windows Masaüstü Uygulaması Yol Haritası

## Proje Özeti
Windows masaüstü uygulaması - PDF, Excel, Word dosyalarını yükleme ve içeriklerinde arama yapma sistemi.

## Teknoloji Stack
- **Framework**: .NET 8.0 (WPF - Windows Presentation Foundation)
- **Dil**: C#
- **PDF İşleme**: iTextSharp veya PdfPig
- **Excel İşleme**: EPPlus veya ClosedXML
- **Word İşleme**: DocumentFormat.OpenXml
- **Arama Motoru**: Lucene.NET veya basit text matching
- **UI Framework**: WPF (Modern Material Design veya Fluent Design)

## Proje Yapısı

```
DocumentSearch/
├── DocumentSearch.sln
├── DocumentSearch/
│   ├── Models/
│   │   ├── Document.cs
│   │   ├── SearchResult.cs
│   │   └── PriceItem.cs
│   ├── Services/
│   │   ├── IDocumentService.cs
│   │   ├── DocumentService.cs
│   │   ├── ISearchService.cs
│   │   ├── SearchService.cs
│   │   ├── IPdfParser.cs
│   │   ├── PdfParser.cs
│   │   ├── IExcelParser.cs
│   │   ├── ExcelParser.cs
│   │   ├── IWordParser.cs
│   │   └── WordParser.cs
│   ├── ViewModels/
│   │   ├── MainViewModel.cs
│   │   └── SearchViewModel.cs
│   ├── Views/
│   │   ├── MainWindow.xaml
│   │   └── MainWindow.xaml.cs
│   ├── App.xaml
│   ├── App.xaml.cs
│   └── DocumentSearch.csproj
└── README.md
```

## Geliştirme Aşamaları

### Faz 1: Proje Kurulumu ve Temel Yapı
1. ✅ .NET 8.0 WPF projesi oluştur
2. ✅ NuGet paketlerini yükle:
   - PdfPig (PDF okuma)
   - EPPlus (Excel okuma)
   - DocumentFormat.OpenXml (Word okuma)
   - CommunityToolkit.Mvvm (MVVM pattern)
3. ✅ Proje klasör yapısını oluştur (Models, Services, ViewModels, Views)

### Faz 2: Veri Modelleri
1. ✅ `PriceItem` modeli (Poz No, Tanım, Birim, Fiyat)
2. ✅ `Document` modeli (Dosya bilgileri, içerik, parse edilmiş veriler)
3. ✅ `SearchResult` modeli (Arama sonuçları için)

### Faz 3: Dosya Yükleme ve Parsing
1. ✅ Dosya seçme dialog'u (PDF, Excel, Word)
2. ✅ PDF parser servisi (tablo verilerini çıkarma)
3. ✅ Excel parser servisi
4. ✅ Word parser servisi
5. ✅ Yüklenen dosyaları saklama (memory veya local storage)

### Faz 4: Arama Motoru
1. ✅ Arama servisi oluştur
2. ✅ Dosya isimlerinde arama
3. ✅ İçeriklerde arama (Poz No, Tanım alanlarında)
4. ✅ Case-insensitive ve Türkçe karakter desteği
5. ✅ Fuzzy search (yaklaşık eşleşme)

### Faz 5: Kullanıcı Arayüzü
1. ✅ Ana pencere tasarımı
2. ✅ Dosya yükleme butonu ve alanı
3. ✅ Arama input alanı
4. ✅ Sonuç listesi (DataGrid veya ListView)
5. ✅ Sonuçlarda dosya adı, poz no, tanım gösterimi
6. ✅ Modern ve kullanıcı dostu UI

### Faz 6: Özellikler ve İyileştirmeler
1. ✅ Yüklenen dosya listesi gösterimi
2. ✅ Dosya silme özelliği
3. ✅ Arama sonuçlarında highlight
4. ✅ Dosya içeriğini görüntüleme (detay penceresi)
5. ✅ Hata yönetimi ve kullanıcı bildirimleri
6. ✅ Loading indicator

### Faz 7: Test ve Optimizasyon
1. ✅ Farklı dosya formatlarında test
2. ✅ Büyük dosyalarda performans testi
3. ✅ Arama performansı optimizasyonu
4. ✅ Memory leak kontrolü

## Detaylı Özellikler

### Dosya Yükleme
- Drag & Drop desteği
- Çoklu dosya seçimi
- Desteklenen formatlar: .pdf, .xlsx, .xls, .docx, .doc
- Yüklenen dosyaların listelenmesi
- Dosya bilgileri (isim, boyut, yükleme tarihi)

### Arama Özellikleri
- **Poz No Arama**: Tam eşleşme veya kısmi eşleşme
- **Tanım Arama**: Kelime bazlı arama, Türkçe karakter desteği
- **Dosya İsmi Arama**: Dosya adlarında arama
- **Kombine Arama**: Tüm alanlarda arama
- **Real-time Arama**: Yazarken sonuçları güncelleme (opsiyonel)

### Sonuç Gösterimi
- Dosya adı
- Poz No
- Tanım
- Birim
- Fiyat (varsa)
- Eşleşme skoru veya highlight

## Teknik Detaylar

### PDF Parsing
- Tablo yapısını algılama
- Poz No, Tanım, Birim, Fiyat kolonlarını otomatik tespit
- Çok sayfalı PDF desteği

### Excel Parsing
- Sheet bazlı okuma
- Tablo formatını algılama
- Header row tespiti

### Word Parsing
- Tablo içeren Word dosyalarını okuma
- Format korunması

### Arama Algoritması
- Index-based arama (hızlı sonuç için)
- Türkçe karakter normalizasyonu (ı->i, ş->s, etc.)
- Levenshtein distance (fuzzy matching)

## Kullanıcı Deneyimi (UX)
- Modern, temiz arayüz
- Responsive tasarım
- Hızlı arama sonuçları
- Kullanıcı dostu hata mesajları
- Keyboard shortcuts (Ctrl+F, Ctrl+O, etc.)

## Gelecek Geliştirmeler (Opsiyonel)
- Veritabanı entegrasyonu (SQLite)
- Export özelliği (sonuçları Excel'e aktarma)
- Filtreleme seçenekleri
- Arama geçmişi
- Favoriler
- Dark mode

