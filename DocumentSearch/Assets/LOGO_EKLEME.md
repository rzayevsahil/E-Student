# Logo Dosyası Ekleme

## Logo Dosyası Konumu

Logo dosyanızı şu konuma koyun:
```
DocumentSearch/Assets/logo.png
```

## Logo Gereksinimleri

- **Format**: PNG (şeffaf arka plan önerilir)
- **Boyut**: 40x40 piksel veya daha büyük (otomatik ölçeklenecek)
- **Önerilen**: 80x80 veya 128x128 piksel (daha net görünüm için)

## Logo Dosyası Ekleme Adımları

1. Logo dosyanızı hazırlayın (PNG formatında)
2. `DocumentSearch/Assets/` klasörüne `logo.png` olarak kaydedin
3. Eğer farklı bir isim kullanmak isterseniz, `MainWindow.xaml` dosyasındaki `Source="Assets/logo.png"` kısmını güncelleyin

## Alternatif: Logo Dosyası Yoksa

Eğer henüz logo dosyanız yoksa, geçici olarak emoji veya ikon kullanabilirsiniz. `MainWindow.xaml` dosyasında `Image` kontrolünü kaldırıp sadece metin kullanabilirsiniz.

## Dosya Yapısı

```
DocumentSearch/
  ├── Assets/
  │   └── logo.png  ← Logo dosyanızı buraya koyun
  ├── MainWindow.xaml
  └── ...
```
