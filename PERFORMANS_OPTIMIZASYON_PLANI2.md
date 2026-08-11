# 🚀 DocumentSearch Performans & Hızlandırma Planı

## 📌 Genel Bakış
Bu doküman, uygulamaya çok sayıda belge (PDF, Excel, Word) yüklendikten sonra yaşanan **geç açılma**, **arama yaparken donma** ve **yüksek işlemci/bellek kullanımı** sorunlarını çözmek için hazırlanan adım adım performans optimizasyon planını içerir.

---

## 🔍 Mevcut Mimarideki Performans Darboğazları

| # | Darboğaz (Bottleneck) | Mevcut Durum | Etkisi |
|---|----------------------|--------------|--------|
| **1** | **Yeniden Parsing (Re-parsing)** | Uygulama her açıldığında kayıtlı tüm dosyaları diske gidip sıfırdan parse eder. | Açılış süresi dosya sayısıyla orantılı olarak katlanarak uzar. |
| **2** | **UI Thread'de Arama** | Arama işlemi arayüz thread'inde (`Dispatcher`) senkron çalışır. | Yazı yazarken arayüzde donma ve takılmalar yaşanır. |
| **3** | **Sıralı İşleme (Sequential Processing)** | Çoklu dosya yüklemeleri `foreach` ile sırayla yapılır. | Çok çekirdekli işlemci gücü kullanılmaz. |
| **4** | **Lineer Metin Taraması** | Her aramada tüm metinler `IndexOf` ile baştan sona taranır. | Büyük metinlerde arama süresi uzar. |

---

## 🛠️ Adım Adım Optimizasyon Yol Haritası

### 📍 Aşama 1: Metin Önbellekleme Sistemi (Local Text Caching) — *En Yüksek Öncelik*
> **Hedef:** Uygulama açılış süresini dakikalardan **milisaniyelere** düşürmek.

* **Yapılacaklar:**
  1. Çıkarılan ham metinleri (`RawContent`) `AppData/Local/DocumentSearch/Cache` klasörüne dosya karması (hash) veya yol bilgisiyle kaydetmek.
  2. Dosyanın son değiştirilme tarihini (`LastWriteTime`) ve boyutunu saklamak.
  3. Açılışta:
     - Dosya değişmediyse ➔ **Doğrudan cache'den oku** (Sıfır PDF/Excel okuma maliyeti).
     - Dosya güncellendiyse ➔ Tekrar parse et ve cache'i yenile.
  4. Silinen dosyaların cache kayıtlarını otomatik temizlemek.

---

### 📍 Aşama 2: Paralel Dosya İşleme (Parallel Processing)
> **Hedef:** Çoklu dosya yüklemelerinde işlem süresini %60-%80 oranında azaltmak.

* **Yapılacaklar:**
  1. `LoadFiles` metodundaki sıralı `foreach` döngüsünü `Parallel.ForEachAsync` veya `Task.WhenAll` yapısına geçirmek.
  2. İşlemci çekirdek sayısına göre eşzamanlı çalışma limitini (`MaxDegreeOfParallelism`) ayarlamak.
  3. İlerleme durumunu (ProgressBar) kullanıcıya canlı olarak bildirmek.

---

### 📍 Aşama 3: Arka Plan Araması & Debounce / Cancellation
> **Hedef:** Arama yaparken kullanıcı arayüzünün (UI) akıcı ve donmasız kalmasını sağlamak.

* **Yapılacaklar:**
  1. Arama işlemini `Task.Run` ile arka plan thread'ine taşımak.
  2. `CancellationTokenSource` entegre ederek, kullanıcı hızlı yazarken önceki arama isteklerini otomatik iptal etmek (Debouncing).
  3. Arama sonuçlarını UI thread'ine performanslı biçimde aktarmak.

---

### 📍 Aşama 4: Tam Metin İndeksleme (SQLite FTS5 / Inverted Index) — *Gelişmiş Seviye*
> **Hedef:** Binlerce sayfalık arşivlerde bile arama süresini <10ms seviyesine indirmek.

* **Yapılacaklar:**
  1. Çıkarılan metinleri SQLite FTS5 (Full-Text Search) tablosunda indekslemek.
  2. Metin içinde kelime bazlı hızlı arama ve Türkçe karakter desteğini FTS5 kütüphanesine uyarlamak.

---

## 📊 Beklenen Performans Artışı (Tahmini)

```
Açılış Süresi (50 Büyük PDF/Excel):
[Mevcut]   ██████████████████████████ 30-45 saniye
[Aşama 1]  █ 0.3 saniye (%99 Hızlanma)

Arama Tepki Süresi (Metin İçi):
[Mevcut]   ██████████ 1.5 - 3.0 saniye (UI donuyor)
[Aşama 3]  █ 0.1 saniye (Akıcı UI)
```

---

## 📋 Önerilen Uygulama Sırası
1. **Aşama 1 (Cache Sistemi)** `DocumentService.cs` güncellenerek hemen başlanabilir.
2. **Aşama 2 (Paralel Yükleme)** `MainViewModel.cs` güncellenerek eklenebilir.
3. **Aşama 3 (Async Arama & İptal)** `SearchService.cs` ve `MainViewModel.cs` güncellenerek tamamlanabilir.