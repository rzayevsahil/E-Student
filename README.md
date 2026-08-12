# E-Student - Akıllı Öğrenci Asistanı & Verimlilik Platformu

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**E-Student**, öğrencilerin ders materyallerini yönetmelerini, dokümanları içerisinde anında arama yapmalarını ve Pomodoro tekniği ile çalışma verimliliklerini artırmalarını sağlayan modern bir Windows masaüstü asistanıdır.

🌐 **Resmi Web Sitesi**: [https://rzayevsahil.github.io/E-Student/](https://rzayevsahil.github.io/E-Student/)

---

## 🌟 Öne Çıkan Özellikler

- 🔍 **Akıllı Belge ve Ders Notu Arama**:
  - PDF, Word (`.docx`, `.doc`), Excel (`.xlsx`, `.xls`) ve PowerPoint (`.pptx`, `.ppt`) dosyalarınızı yükleyin.
  - Ders notları, sunum slaytları, poz numaraları, kelimeler ve tablolar içerisinde saliseler içinde arama yapın.
  - Çok çekirdekli arka plan motoru ve akıllı yerel metin önbellekleme sayesinde binlerce sayfalık ders materyalinde anında arama.

- 🌐 **Çok Dilli Arayüz Desteği (TR | EN | AZ)**:
  - Türkçe, İngilizce ve Azerbaycanca dil desteği.
  - Uygulama içi anında dinamik dil değişimi ve seçilen dilin kalıcı saklanması.

- 👁️ **Hızlı Önizleme Paneli (Split-View Reader)**:
  - Arama sonuçlarını uygulama dışına çıkmadan sağ taraftaki önizleme panelinde sayfa ve slayt bazlı inceleme.
  - Sayfalar arası hızlı gezinme (Önceki/Sonraki sayfa) ve harici uygulamada açma seçeneği.

- 📂 **Sürükle-Bırak & Akıllı Filtreleme**:
  - Masaüstünden veya klasörlerden dosyaları doğrudan uygulamaya sürükleyip bırakarak yükleme.
  - PDF, Word, Excel ve PowerPoint türlerine göre dinamik çip filtreleme ve dosya adı araması.

- ⏱️ **Pomodoro Sayaç & Çalışma Yönetimi**:
  - Odaklanma oturumları (25 dk), kısa (5 dk) ve uzun (15 dk) mola zamanlayıcısı.
  - Tamamlanan Pomodoro istatistikleri ve özelleştirilebilir eylem butonları.

- ⚡ **Yüksek Performans & Metin Önbellekleme**:
  - Belgelerin içerikleri ilk taramada önbelleğe alınır, sonraki açılışlar milisaniyeler sürer.

- 🔄 **Otomatik Güncelleme & Kolay Kurulum**:
  - Inno Setup altyapısı ile hızlı kurulum sihirbazı.
  - GitHub Releases üzerinden güncellemeleri otomatik denetleme ve arka planda sessiz güncelleme.

- 🎨 **Modern ve Temiz Kullanıcı Arayüzü**:
  - Kullanıcı dostu, modern WPF arayüzü, karanlık mod desteği ve sezgisel gezinme menüsü.

---

## 🛠️ Teknoloji Stack

- **.NET 8.0 (Windows)** - Framework
- **WPF** - UI Framework
- **PdfPig** - PDF işleme
- **ClosedXML** - Excel işleme
- **DocumentFormat.OpenXml** - Word ve PowerPoint işleme
- **Inno Setup** - Windows Kurulum Sihirbazı
- **CommunityToolkit.Mvvm** - MVVM Pattern
- **Microsoft.Extensions.DependencyInjection** - Dependency Injection

---

## 📦 İndirme ve Kurulum

### 🚀 Kullanıcılar İçin (Hızlı Kurulum)

1. [GitHub Releases](https://github.com/rzayevsahil/E-Student/releases) sayfasından en son sürüm **`E-Student-Setup-vX.X.X.exe`** kurulum dosyasını indirin.
2. İndirdiğiniz `.exe` dosyasına çift tıklayarak kurulum adımlarını takip edin.
3. Masaüstünüzde veya Başlat Menüsünde oluşan **E-Student** simgesi ile uygulamayı başlatın.

### 💻 Geliştiriciler İçin (Kaynak Koddan Derleme)

1. Projeyi klonlayın:
   ```bash
   git clone https://github.com/rzayevsahil/E-Student.git
   cd E-Student
   ```
2. Projeyi derleyin:
   ```bash
   dotnet build DocumentSearch/DocumentSearch.csproj
   ```
3. Uygulamayı çalıştırın:
   ```bash
   dotnet run --project DocumentSearch/DocumentSearch.csproj
   ```

---

## 💡 Kullanım Rehberi

1. **Belge Arama & Ders Notları**: 
   - "Dosya Yükle" ile PDF, Word, Excel ve PowerPoint formatındaki ders notlarınızı ve kaynaklarınızı ekleyin.
   - Arama çubuğuna aradığınız kelimeyi veya ifadeyi yazın; sonuçlar ilgili sayfa ve konumla anında listelenir.

2. **Pomodoro Odaklanma Modu**:
   - Sol menüden Pomodoro sekmesine geçin, çalışma sürenizi başlatın ve mola aralıklarınızı düzenleyin.

---

## 📄 Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına göz atabilirsiniz.
