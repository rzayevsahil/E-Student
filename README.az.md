# E-Student - Ağıllı Tələbə Köməkçisi & Məhsuldarlıq Platforması

[🇹🇷 Türkçe](README.md) | [🇬🇧 English](README.en.md) | [🇦🇿 Azərbaycanca](README.az.md)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**E-Student**, tələbələrin dərs materiallarını idarə etmələrini, sənədlərin içərisində anında axtarış etmələrini və Pomodoro texnikası ilə çalışma məhsuldarlığını artırmalarını təmin edən müasir Windows masaüstü köməkçisidir.

🌐 **Rəsmi Veb Sayt**: [https://rzayevsahil.github.io/E-Student/](https://rzayevsahil.github.io/E-Student/)

---

## 🌟 Əsas Xüsusiyyətlər

- 🔍 **Ağıllı Sənəd və Dərs Qeydi Axtarışı**:
  - PDF, Word (`.docx`, `.doc`), Excel (`.xlsx`, `.xls`), PowerPoint (`.pptx`, `.ppt`) və Şəkil (`.png`, `.jpg`, `.jpeg`, `.bmp`, `.tiff`) fayllarınızı yükləyin.
  - Dərs qeydləri, təqdimat slaydları, istilahlar, rəqəmlər və cədvəllər içərisində salisələr içində axtarış edin.
  - Çoxnüvəli arxa plan mühərriki və ağıllı yerli mətn keşləməsi sayəsində minlərlə səhifəlik dərs materialında anında axtarış.

- 🖼️ **Yerli OCR Dəstəyi (Şəkil Daxilində Mətn Axtarışı)**:
  - Dərs qeydi şəkilləri, ekran görüntüləri və skan edilmiş sənədlər (`.png`, `.jpg`, `.jpeg`, `.bmp`, `.tiff`) içərisindəki mətnləri Windows Native OCR texnologiyası ilə avtomatik aşkar etmə və axtarma imkanı.

- ⭐ **Sevimlilər & Teqləmə (Tagging) Sistemi**:
  - Vacib sənədləri sevimlilərə (⭐) əlavə etmə və tək kliklə filtrləmə.
  - Sənədlərə xüsusi teqlər (`#Riyaziyyat101`, `#İmtahan`, `#Laboratoriya`) əlavə edərək fənn bazlı qruplaşdırma və teq adı ilə axtarış etmə imkanı.

- 🌐 **Çoxdilli İnterfeys Dəstəyi (AZ | EN | TR)**:
  - Azərbaycan, İngilis və Türk dillərində tam interfeys dəstəyi.
  - Tətbiq daxilində tək kliklə anında dinamik dil dəyişməsi və seçilmiş dilin qalıcı saxlanılması.

- 👁️ **Tez Önizləmə Paneli (Split-View Reader)**:
  - Axtarış nəticələrini tətbiqdən çıxmadan sağ tərəfdəki önizləmə panelində səhifə və slayd bazlı nəzərdən keçirmək imkanı.
  - Səhifələr arasında sürətli keçid (Əvvəlki/Növbəti səhifə) və xarici tətbiqdə açmaq seçimi.

- 📂 **Sürüklə-Burax & Ağıllı Filtrləmə**:
  - Masaüstündən və ya qovluqlardan faylları birbaşa tətbiqə sürükləyib buraxaraq yükləmə.
  - PDF, Word, Excel və PowerPoint növlərinə görə dinamik çip filtrləməsi və fayl adı axtarışı.

- ⏱️ **Pomodoro Taymeri & Çalışma İdarəetməsi**:
  - Diqqət seansları (25 dəq), qısa (5 dəq) və uzun (15 dəq) fasilə taymeri.
  - Tamamlanmış Pomodoro statistikaları və özümləşdirilə bilən fəaliyyət düymələri.

- ⚡ **Yüksək Məhsuldarlıq & Mətn Keşləməsi**:
  - Sənədlərin məzmunu ilk taramada keşə alınır, növbəti açılışlar milisaniyələr çəkir.

- 🔄 **Avtomatik Yenilənmə & Asan Quraşdırma**:
  - Inno Setup altyapısı ilə sürətli quraşdırma sehrbazı.
  - GitHub Releases üzərindən yenilənmələri avtomatik yoxlama və arxa planda səssiz yenilənmə.

- 🎨 **Müasir və Səliqəli İstifadəçi İnterfeysi**:
  - İstifadəçi dostu müasir WPF interfeysi, qaranlıq rejim dəstəyi və rahat naviqasiya menyusu.

---

## 🛠️ Texnologiya Staki

- **.NET 8.0 (Windows)** - Framework
- **WPF** - UI Framework
- **PdfPig** - PDF Emalı
- **ClosedXML** - Excel Emalı
- **DocumentFormat.OpenXml** - Word & PowerPoint Emalı
- **Inno Setup** - Windows Quraşdırma Sehrbazı
- **CommunityToolkit.Mvvm** - MVVM Pattern
- **Microsoft.Extensions.DependencyInjection** - Dependency Injection

---

## 📦 Yükləmə və Quraşdırma

### 🚀 İstifadəçilər Üçün (Hızlı Quraşdırma)

1. [GitHub Releases](https://github.com/rzayevsahil/E-Student/releases) səhifəsindən ən son **`E-Student-Setup-vX.X.X.exe`** quraşdırma faylını yükləyin.
2. Yüklədiyiniz `.exe` faylına iki dəfə klikləyərək quraşdırma addımlarını izləyin.
3. Masaüstünüzdə və ya Başlat Menyusunda yaranan **E-Student** ikonu ilə tətbiqi başladın.

### 💻 Tərtibatçılar Üçün (Mənbə Kodundan Yığma)

1. Layihəni klonlayın:
   ```bash
   git clone https://github.com/rzayevsahil/E-Student.git
   cd E-Student
   ```
2. Layihəni yığın:
   ```bash
   dotnet build DocumentSearch/DocumentSearch.csproj
   ```
3. Tətbiqi işə salın:
   ```bash
   dotnet run --project DocumentSearch/DocumentSearch.csproj
   ```

---

## 💡 İstifadə Təlimatı

1. **Sənəd Axtarışı & Dərs Qeydləri**: 
   - "Fayl Yüklə" düyməsi ilə PDF, Word, Excel və PowerPoint formatındakı dərs qeydlərinizi əlavə edin.
   - Axtarış zolağına axtardığınız sözü yazın; nəticələr müvafiq səhifə və slayd nömrəsi ilə anında siyahılanacaq.

2. **Pomodoro Rejimi**:
   - Sol menyudan Pomodoro seksekəsinə keçin, çalışma vaxtınızı başladın və fasilə intervallarınızı tənzimləyin.

---

## 📄 Lisenziya

Bu layihə [MIT Lisenziyası](LICENSE) altında lisenziyalaşdırılmışdır. Ətraflı məlumat üçün [LICENSE](LICENSE) faylına baxa bilərsiniz.
