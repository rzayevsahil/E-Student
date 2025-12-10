# app.ico Dosyası Oluşturma

## Dosya Konumu
`app.ico` dosyasını şu konuma koyun:
```
DocumentSearch/
  └── app.ico  ← Buraya
```

## .ico Dosyası Oluşturma Yöntemleri

### Yöntem 1: Online Converter (En Kolay)
1. Herhangi bir PNG veya JPG görseli hazırlayın (256x256 veya 512x512 piksel önerilir)
2. Online converter kullanın:
   - https://convertio.co/tr/png-ico/
   - https://www.icoconverter.com/
   - https://www.favicon-generator.org/
3. Oluşturulan `app.ico` dosyasını `DocumentSearch` klasörüne kopyalayın

### Yöntem 2: Visual Studio ile
1. Visual Studio'da projeye sağ tıklayın
2. **Add** > **New Item** > **Icon File** seçin
3. Adını `app.ico` yapın
4. İkonu düzenleyin

### Yöntem 3: Paint.NET veya GIMP
1. 256x256 veya 512x512 piksel bir görsel oluşturun
2. ICO formatında kaydedin
3. `DocumentSearch` klasörüne `app.ico` olarak kaydedin

### Yöntem 4: PowerShell ile Basit ICO (Geçici)
Eğer hızlıca test etmek istiyorsanız, aşağıdaki komutu çalıştırabilirsiniz (basit bir placeholder oluşturur):

```powershell
# Bu komut basit bir placeholder .ico oluşturur
# Gerçek bir ikon için yukarıdaki yöntemleri kullanın
```

## Önemli Notlar
- ICO dosyası en az 16x16, 32x32, 48x48, 256x256 boyutlarını içermelidir
- Dosya adı tam olarak `app.ico` olmalıdır
- Dosya `DocumentSearch` klasöründe (csproj dosyasıyla aynı yerde) olmalıdır

## Dosya Yapısı
```
DocumentSearch/
  ├── DocumentSearch.csproj  ← Burada ApplicationIcon>app.ico</ApplicationIcon> var
  ├── app.ico                ← Buraya koyun
  ├── MainWindow.xaml
  └── ...
```

