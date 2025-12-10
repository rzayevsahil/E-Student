# DocumentSearch - Belge Arama Uygulaması

Windows masaüstü uygulaması - PDF, Excel ve Word dosyalarını yükleme ve içeriklerinde arama yapma sistemi.

## Özellikler

- ✅ **Çoklu Dosya Formatı Desteği**: PDF, Excel (.xlsx, .xls) ve Word (.docx, .doc) dosyalarını yükleme
- ✅ **Akıllı Arama**: 
  - Poz No ile arama
  - Tanım ifadelerine göre arama
  - Dosya isimlerinde arama
  - Türkçe karakter desteği (ı/İ, ş/Ş, ğ/Ğ, ü/Ü, ö/Ö, ç/Ç)
- ✅ **Tablo Verisi Çıkarma**: PDF, Excel ve Word dosyalarındaki tablo verilerini otomatik olarak parse etme
- ✅ **Modern UI**: WPF ile modern ve kullanıcı dostu arayüz
- ✅ **Real-time Arama**: Yazarken anında arama sonuçları

## Teknoloji Stack

- **.NET 8.0** - Framework
- **WPF** - UI Framework
- **PdfPig** - PDF işleme
- **EPPlus** - Excel işleme
- **DocumentFormat.OpenXml** - Word işleme
- **CommunityToolkit.Mvvm** - MVVM pattern
- **Microsoft.Extensions.DependencyInjection** - Dependency Injection

## Kurulum

1. Projeyi klonlayın veya indirin
2. Terminal'de proje klasörüne gidin:
   ```bash
   cd DocumentSearch
   ```
3. Projeyi derleyin:
   ```bash
   dotnet build
   ```
4. Uygulamayı çalıştırın:
   ```bash
   dotnet run
   ```

## Kullanım

1. **Dosya Yükleme**: 
   - "Dosya Yükle" butonuna tıklayın
   - PDF, Excel veya Word dosyalarınızı seçin
   - Çoklu dosya seçimi desteklenir

2. **Arama**:
   - Arama kutusuna Poz No (örn: 15.100.1001) veya tanım ifadesi yazın
   - Sonuçlar anında görüntülenecektir
   - Arama, dosya isimlerinde, Poz No'larda ve tanımlarda yapılır

3. **Sonuçları Görüntüleme**:
   - Arama sonuçları tablo formatında gösterilir
   - Dosya adı, Poz No, Tanım, Birim ve Fiyat bilgileri görüntülenir
   - Eşleşme tipi (PozNo, Tanim, FileName) gösterilir

4. **Dosya Kaldırma**:
   - Yüklenen dosyalar listesindeki ✕ butonuna tıklayarak dosyayı kaldırabilirsiniz

## Proje Yapısı

```
DocumentSearch/
├── Models/          # Veri modelleri (PriceItem, Document, SearchResult)
├── Services/        # İş mantığı servisleri (Parser'lar, Arama servisi)
├── ViewModels/     # MVVM ViewModel'ler
├── Views/          # XAML view dosyaları
└── MainWindow.xaml # Ana pencere
```

## Geliştirme Notları

- PDF parser, tablo formatını otomatik algılamaya çalışır
- Excel parser, header satırını otomatik tespit eder
- Word parser, tablo içeren Word dosyalarını işler
- Arama servisi, Türkçe karakterleri normalize ederek arama yapar
- Tüm servisler dependency injection ile yönetilir

## Lisans

Bu proje eğitim amaçlıdır.

