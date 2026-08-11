# DocumentSearch - Belge Arama Uygulaması

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Windows masaüstü uygulaması - PDF, Excel ve Word dosyalarını yükleme ve içeriklerinde arama yapma sistemi.

## Özellikler

- ✅ **Çoklu Dosya Formatı Desteği**: PDF, Excel (.xlsx, .xls) ve Word (.docx, .doc) dosyalarını yükleme
- ✅ **Akıllı & Hızlı Arama**: 
  - Poz No, kelime, sayfa numarası ve dosya isimlerinde arama
  - Çok çekirdekli (Parallel) arka plan arama motoru
  - Türkçe karakter duyarlılığı ve normalizasyonu (ı/İ, ş/Ş, ğ/Ğ, ü/Ü, ö/Ö, ç/Ç)
- ✅ **Yüksek Performans & Metin Önbellekleme**: Belgelerin metin içerikleri ilk yüklemede önbelleğe alınır, uygulama açılışları milisaniyeler sürer.
- ✅ **Otomatik Güncelleme & Kurulum Sihirbazı**: Güncel sürümleri GitHub üzerinden kontrol etme ve Inno Setup ile tek tıkla arka planda güncelleme.
- ✅ **Modern UI**: WPF ile temiz, ortalanmış ve modern kullanıcı arayüzü.

## Teknoloji Stack

- **.NET 8.0 (Windows)** - Framework
- **WPF** - UI Framework
- **PdfPig** - PDF işleme
- **ClosedXML** - Excel işleme
- **DocumentFormat.OpenXml** - Word işleme
- **Inno Setup** - Windows Kurulum Sihirbazı
- **CommunityToolkit.Mvvm** - MVVM pattern
- **Microsoft.Extensions.DependencyInjection** - Dependency Injection

## İndirme ve Kurulum

### 🚀 Kullanıcılar İçin (Hızlı Kurulum)

1. [GitHub Releases](https://github.com/rzayevsahil/E-Student/releases) sayfasından en son sürüme ait **`DocumentSearch-Setup-vX.X.X.exe`** kurulum dosyasını indirin.
2. İndirdiğiniz `.exe` dosyasına çift tıklayarak kurulum sihirbazını adımlarını takip edin.
3. Kurulum tamamlandıktan sonra Masaüstünüzde veya Başlat Menüsünde oluşan **E-Student** kısayolu ile uygulamayı çalıştırabilirsiniz.

### 💻 Geliştiriciler İçin (Kaynak Koddan Derleme)

1. Projeyi klonlayın:
   ```bash
   git clone https.github.com/rzayevsahil/E-Student.git
   cd E-Student
   ```
2. Projeyi derleyin:
   ```bash
   dotnet build
   ```
3. Uygulamayı çalıştırın:
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

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına göz atabilirsiniz.

