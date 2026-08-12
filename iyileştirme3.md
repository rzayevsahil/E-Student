Pomodoro sayfasındaki alt kart yapısında ciddi bir dikey alan (vertical space) sıkışması var. *"Belge Bazlı Çalışma Süreleri"* kartı dar bir yüksekliğe hapsedildiği için alt taraftaki *"Pomodoro Tekniği Nasıl Çalışır?"* kartı kesilmiş ve sağda kalın bir scrollbar belirmiş.

Bu alanı hem estetik hem de işlevsel kılmak için uygulayabileceğin en pratik tasarım çözümleri:

---

### 1. Tabbed Card (Sekmeli Kart) Yapısına Geçmek (En Temiz Çözüm)

Bu iki farklı içeriği dikeyde üst üste yığmak yerine, alt alanı **tek bir ana kart** yapıp üstüne 2 sekme (Tab) ekleyebilirsin:

* **Sekme 1:** 📊 Çalışma İstatistikleri / Belge Süreleri
* **Sekme 2:** ❓ Pomodoro Nedir & Nasıl Çalışır?

**Avantajı:** Dikey alanı taşırıp scrollbar çıkarmak yerine, kullanıcı hangisini görmek istiyorsa tek tıkla ona geçer. Kartın yüksekliği sabit ve şık kalır.

---

### 2. Akordeon (Accordion / Collapsible) Kullanmak

Bilgilendirme metni olan *"Pomodoro Tekniği Nasıl Çalışır?"* her zaman görünmek zorunda olmayan ikincil bir bilgidir.

* Bu bölümü varsayılan olarak **kapalı (collapsed)** bir akordeon yapabilirsin.
* Sadece başlık ve sağında küçük bir aşağı ok ikonu (`▼`) durur. Kullanıcı tıkladığında aşağı doğru süzülerek açılır.
* Böylece *"Belge Bazlı Çalışma Süreleri"* kartı rahatça genişler ve scrollbar ortadan kalkar.

---

### 3. Bilgi Metnini Modal / Popover İle Ayırmak

"Pomodoro nasıl çalışır?" rehberi her gün uygulamayı kullanan bir öğrenci için bir süre sonra gereksiz ekran kaplayan bir bilgiye dönüşür.

* Sağ üst köşeye veya Pomodoro başlığının yanına küçük bir **`[ ? ]` Yardım / Bilgi ikonu** koyabilirsin.
* Tıklandığında şık bir popup/modal açılır veya sağ taraftan açılan bir bilgilendirme paneli gelir.
* Alt alan tamamen **Çalışma İstatistikleri / Belge Süreleri** listesine ayrılmış olur.

---

### 4. Mevcut Düzeni Koruyacaksan Yapılacak Küçük Touch-up'lar

Eğer mevcut 2 kartlı düzeni tutmak istiyorsan:

* **İç Scrollbar'ı Kaldır:** Dış container'a sabit bir height verip içine scroll vermek yerine, sayfanın geneline taşmasına izin ver (veya kart yüksekliğini `min-height` ile içeriğe göre esnet).
* **Grid / 2 Kolon Yapısı:** Ekran genişliği oldukça fazla. Alt alanı dikeyde üst üste koymak yerine **sol tarafa** *Belge Bazlı Çalışma Süreleri* (%60 genişlik), **sağ tarafa** *Nasıl Çalışır?* (%40 genişlik) gelecek şekilde yan yana (2 kolonlu) dizebilirsin.

> **Özet Tavsiye:** En iyi UX için **1. Yöntem (Sekmeli Kart)** veya **4. Yöntemdeki 2 Kolonlu (Side-by-Side) Düzen** bu boş beyaz alandan maksimum verimi almanı sağlar.