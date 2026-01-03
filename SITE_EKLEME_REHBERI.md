# 🌐 Site Ekleme Rehberi

<div align="center">

**Manuel ve otomatik site ekleme adım adım rehberi**

</div>

---

## 📋 İçindekiler

1. [Site Tipleri](#1-site-tipleri)
2. [Otomatik Desteklenen Siteler](#2-otomatik-desteklenen-siteler)
3. [Manuel Site Ekleme](#3-manuel-site-ekleme)
4. [XPath Seçici Yazma](#4-xpath-seçici-yazma)
5. [CSS Seçici Yazma](#5-css-seçici-yazma)
6. [Örnek Site Konfigürasyonları](#6-örnek-site-konfigürasyonları)
7. [Sorun Giderme](#7-sorun-giderme)
8. [Gelişmiş Teknikler](#8-gelişmiş-teknikler)

---

## 1. Site Tipleri

### 1.1 AutoSupported (Otomatik Desteklenen)

Özel adapter yazılmış siteler. Sadece URL girmeniz yeterli.

| Site | Adapter | Özellik |
|------|---------|---------|
| Youthall | `YouthallAdapter` | REST API kullanır |
| İlanburda | `IlanburdaAdapter` | HTML parsing |
| Microfon | `MicrofonAdapter` | Next.js JSON parsing |

**Ne zaman kullanılır?**
- Yukarıdaki siteler için
- Adapter'ı bulunan tüm siteler

### 1.2 Manual (Manuel)

XPath veya CSS seçicilerle özel konfigürasyon gerektiren siteler.

**Ne zaman kullanılır?**
- Listede olmayan siteler
- Özel yapıya sahip siteler
- Hızlı test için

---

## 2. Otomatik Desteklenen Siteler

### 2.1 Youthall Ekleme

```
Site Adı:         Youthall
Base URL:         https://youthall.com/tr/talent-programs/
Site Tipi:        AutoSupported
Aktif:            ✅
Kontrol Aralığı:  10 dakika
```

> ⚠️ Youthall REST API kullandığı için parser ayarları gerekmez.

### 2.2 İlanburda Ekleme

```
Site Adı:         İlanburda
Base URL:         https://ilanburda.net/8/is-ilanlari
Site Tipi:        AutoSupported
Aktif:            ✅
Kontrol Aralığı:  10 dakika
```

### 2.3 Microfon Ekleme

```
Site Adı:         Microfon
Base URL:         https://microfon.co/scholarship
Site Tipi:        AutoSupported
Aktif:            ✅
Kontrol Aralığı:  30 dakika
```

> ⚠️ Microfon Next.js kullandığı için `__NEXT_DATA__` JSON'dan parse edilir.

---

## 3. Manuel Site Ekleme

### 3.1 Genel Akış

```
┌─────────────────────────────────────────────────────────┐
│ 1. Site Analizi                                         │
│    └─ Tarayıcıda F12 → Elements                         │
├─────────────────────────────────────────────────────────┤
│ 2. DOM Yapısını Anlama                                  │
│    └─ İlan listesi container'ını bul                    │
│    └─ Tekil ilan elementini bul                         │
│    └─ Başlık, URL, fiyat, şehir elementlerini bul       │
├─────────────────────────────────────────────────────────┤
│ 3. XPath/CSS Seçicileri Yaz                             │
│    └─ Her alan için seçici oluştur                      │
├─────────────────────────────────────────────────────────┤
│ 4. Uygulamada Test Et                                   │
│    └─ Site ekle → Manuel scrape → Sonuçları kontrol et  │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Adım 1: Site Analizi

1. Hedef siteyi tarayıcıda aç
2. **F12** tuşuna bas (Developer Tools)
3. **Elements** sekmesine git
4. İlan listesini bul

**Örnek DOM Yapısı (Eleman.net):**
```html
<div class="ilan_listeleme_bol">
    <a href="/is-ilani/teknik-personel-i4555881">
        <h3 class="c-showcase-box__title">Teknik Personel</h3>
        <div class="c-showcase-box__company">ABC Tech</div>
        <div class="c-showcase-box__location">İstanbul</div>
    </a>
</div>
<div class="ilan_listeleme_bol">
    <a href="/is-ilani/yazilim-gelistirici-i4555882">
        <h3 class="c-showcase-box__title">Yazılım Geliştirici</h3>
        <!-- ... -->
    </a>
</div>
```

### 3.3 Adım 2: Seçicileri Belirle

**Liste Seçici (ListingItemSelector):**
```xpath
//div[contains(@class,'ilan_listeleme_bol')]
```
> Her bir ilan kartını seçer. `contains()` kullanarak partial class match yapabilirsiniz.

**Başlık Seçici (TitleSelector):**
```xpath
.//h3[contains(@class,'c-showcase-box__title')]
```
> Ön `.//` mevcut element içinde arar (ÖNEMLİ!)

**URL Seçici (UrlSelector):**
```xpath
.//a
```
> Link elementinin href'i otomatik alınır. `<a>` etiketi bulunursa href attribute'u çekilir.

**ID Seçici (ListingIdSelector):**
```xpath
.//a
```
> Unique ID için URL kullanılır. ManualSiteScraper URL'den otomatik ID oluşturur (örn: `eleman_4555881`).

### 3.4 Adım 3: Uygulamaya Ekle

```
Siteler → ➕ Site Ekle

┌─────────────────────────────────────────────────────────┐
│ 📋 Temel Bilgiler                                      │
├─────────────────────────────────────────────────────────┤
│ Site Adı:          Eleman.net                          │
│ Base URL:          https://www.eleman.net/is-ilanlari  │
│ Site Tipi:         Manual                              │
│ Seçici Tipi:       XPath                               │
│ ✅ Aktif                                               │
│ Kontrol Aralığı:   10 dakika                           │
├─────────────────────────────────────────────────────────┤
│ ⚙️ Parser Ayarları                                     │
├─────────────────────────────────────────────────────────┤
│ Liste Seçici:                                          │
│ ┌───────────────────────────────────────────────────┐  │
│ │ //div[contains(@class,'ilan_listeleme_bol')]      │  │
│ └───────────────────────────────────────────────────┘  │
│                                                         │
│ Başlık Seçici:   .//h3[contains(@class,'c-showcase-box__title')]│
│ URL Seçici:        .//a                                │
│ ID Seçici:         .//a                                │
│ Şirket Seçici:     (boş)                               │
│ Şehir Seçici:      (boş)                               │
│ Tarih Seçici:      (boş)                               │
│ Fiyat Seçici:      (boş)                               │
└─────────────────────────────────────────────────────────┘

                        [İptal]  [💾 Kaydet]
```

### 3.5 Adım 4: Test Et

1. Site kaydettikten sonra
2. **Siteler** sekmesinde siteyi bul
3. **🔄** butonuna tıkla (Manuel Scrape)
4. Terminalde çıktıyı kontrol et:

```
🔄 Eleman.net scraping başlatılıyor...
📊 35 ilan bulundu
✅ Eleman.net scraping tamamlandı
```

5. **İlanlar** sekmesinde sonuçları kontrol et

---

## 4. XPath Seçici Yazma

### 4.1 Temel XPath Sözdizimi

| Sözdizim | Açıklama | Örnek |
|----------|----------|-------|
| `//` | Dökümanın her yerinde ara | `//div` |
| `/` | Direkt çocuk | `div/span` |
| `.//` | Mevcut node'dan ara | `.//a` |
| `@` | Attribute seç | `@href`, `@class` |
| `[]` | Koşul/filtre | `div[@class='item']` |
| `contains()` | İçerik kontrolü | `contains(@class,'item')` |
| `text()` | Metin içeriği | `//span/text()` |
| `[1]` | İlk element | `//div[1]` |
| `[last()]` | Son element | `//div[last()]` |

### 4.2 Yaygın Seçici Örnekleri

**Class ile seçim:**
```xpath
//div[@class='job-item']
//div[contains(@class,'list-item')]  <!-- Partial match -->
```

**ID ile seçim:**
```xpath
//div[@id='job-list']
//*[@id='main-content']
```

**Nested seçim:**
```xpath
//div[@class='container']//a[@class='title']
//ul[@class='jobs']/li/div[@class='info']
```

**Attribute değeri ile:**
```xpath
//a[contains(@href,'job')]
//input[@type='text']
```

**Text içeriği ile:**
```xpath
//span[contains(text(),'İstanbul')]
//a[text()='Detay']
```

### 4.3 Önemli İpuçları

1. **Göreceli yol kullan (`.//`):**
   ```xpath
   ❌ //h2/a                  # Döküman genelinde arar
   ✅ .//h2/a                 # Mevcut ilan içinde arar
   ```

2. **Boşluk ve büyük/küçük harf:**
   ```xpath
   # HTML: <div class="Job Item">
   ❌ //div[@class='job item']
   ✅ //div[@class='Job Item']
   ✅ //div[contains(@class,'Job')]
   ```

3. **Dinamik class'lar için contains:**
   ```xpath
   # HTML: <div class="item-abc123">
   ✅ //div[contains(@class,'item-')]
   ```

---

## 5. CSS Seçici Yazma

### 5.1 Temel CSS Sözdizimi

| Sözdizim | Açıklama | Örnek |
|----------|----------|-------|
| `.` | Class seçici | `.job-item` |
| `#` | ID seçici | `#job-list` |
| ` ` | Descendant | `div .title` |
| `>` | Direct child | `ul > li` |
| `[attr]` | Attribute var | `[href]` |
| `[attr=value]` | Attribute eşit | `[type='text']` |
| `:first-child` | İlk çocuk | `li:first-child` |
| `:nth-child(n)` | N. çocuk | `li:nth-child(2)` |

### 5.2 CSS Seçici Örnekleri

```css
/* Class ile */
.job-list .job-item

/* ID ile */
#main-content .listing

/* Attribute ile */
a[href*='job']

/* Kombinasyon */
div.container > ul.items > li.item

/* Nested */
.job-card .title a
```

### 5.3 XPath vs CSS

| Özellik | XPath | CSS |
|---------|-------|-----|
| Attribute seçimi | `@href` | Desteklenmez |
| Parent seçimi | `..` | Desteklenmez |
| Text seçimi | `text()` | Desteklenmez |
| Koşullu seçim | `[condition]` | Sınırlı |
| Öğrenme eğrisi | Dik | Kolay |

> **Tavsiye:** Karmaşık seçimler için XPath, basit seçimler için CSS kullanın.

---

## 6. Örnek Site Konfigürasyonları

### 6.1 Eleman.net (XPath) ✅ Test Edildi

```yaml
Site Adı:         Eleman.net
Base URL:         https://www.eleman.net/is-ilanlari
Site Tipi:        Manual
Seçici Tipi:      XPath

Parser Ayarları:
  Liste Seçici:    //div[contains(@class,'ilan_listeleme_bol')]
  Başlık Seçici:   .//h3[contains(@class,'c-showcase-box__title')]
  URL Seçici:      .//a
  ID Seçici:       .//a
  Şirket Seçici:   (boş)
  Şehir Seçici:    (boş)
  Tarih Seçici:    (boş)
  Fiyat Seçici:    (boş)
  Encoding:        UTF-8
```

**DOM Yapısı:**
```html
<div class="ilan_listeleme_bol">
    <a href="/is-ilani/teknik-personel-i4555881">
        <h3 class="c-showcase-box__title">Teknik Personel</h3>
        <!-- diğer içerikler -->
    </a>
</div>
```

> ⚠️ **Önemli**: Eleman.net sayfalama için `?sy=2` formatını kullanır. ManualSiteScraper otomatik olarak bu formatı destekler.

### 6.2 Kariyer.net Benzeri Site (XPath)

```yaml
Site Adı:         KariyerSite
Base URL:         https://example.com/jobs
Site Tipi:        Manual
Seçici Tipi:      XPath

Parser Ayarları:
  Liste Seçici:    //div[@class='job-list']/div[@class='job-card']
  Başlık Seçici:   .//a[@class='job-title']
  URL Seçici:      .//a[@class='job-title']
  ID Seçici:       .//a[@class='job-title']/@href
  Şirket Seçici:   .//span[@class='company']
  Şehir Seçici:    .//span[@class='location']
  Tarih Seçici:    .//span[@class='date']
  Fiyat Seçici:    .//span[@class='salary']
```

### 6.3 Tablo Yapılı Site (XPath)

```yaml
Site Adı:         TableSite
Base URL:         https://example.com/listings
Site Tipi:        Manual
Seçici Tipi:      XPath

Parser Ayarları:
  Liste Seçici:    //table[@class='listings']//tr[@class='row']
  Başlık Seçici:   .//td[1]/a
  URL Seçici:      .//td[1]/a
  ID Seçici:       .//td[1]/a/@href
  Şirket Seçici:   .//td[2]
  Şehir Seçici:    .//td[3]
  Tarih Seçici:    .//td[4]
  Fiyat Seçici:    .//td[5]
```

**DOM Yapısı:**
```html
<table class="listings">
    <tr class="row">
        <td><a href="/job/123">Developer</a></td>
        <td>ABC Tech</td>
        <td>İstanbul</td>
        <td>03.01.2025</td>
        <td>15,000 TL</td>
    </tr>
</table>
```

### 6.4 CSS Seçicili Site

```yaml
Site Adı:         CSSSite
Base URL:         https://example.com/jobs
Site Tipi:        Manual
Seçici Tipi:      CssSelector

Parser Ayarları:
  Liste Seçici:    .job-list .job-item
  Başlık Seçici:   .title a
  URL Seçici:      .title a
  ID Seçici:       .title a
  Şirket Seçici:   .company
  Şehir Seçici:    .location
  Tarih Seçici:    .date
  Fiyat Seçici:    .salary
```

### 6.5 İlanburda Benzeri Site (XPath - Table Row)

```yaml
Site Adı:         IlanSite
Base URL:         https://example.com/ilanlar
Site Tipi:        Manual
Seçici Tipi:      XPath

Parser Ayarları:
  Liste Seçici:    //tr[@class='satir_link']
  Başlık Seçici:   .//a[@class='joblisting']
  URL Seçici:      .//a[@class='joblisting']
  ID Seçici:       .//a[@class='joblisting']/@href
  Şirket Seçici:   
  Şehir Seçici:    .//td[@class='city']
  Tarih Seçici:    .//td[@class='date']
  Fiyat Seçici:    .//td[@class='price']
```

---

## 7. Sorun Giderme

### 7.1 "0 İlan Bulundu" Hatası

**Olası Nedenler:**

1. **Yanlış Liste Seçici:**
   ```bash
   # Kontrol yöntemi:
   # Tarayıcıda Console açın (F12 → Console)
   # XPath için:
   $x("//div[@class='job-list']")
   
   # CSS için:
   document.querySelectorAll(".job-list .job-item")
   ```

2. **Site dinamik JS kullanıyor:**
   - Sayfa yüklendikten sonra içerik render ediliyor
   - Manuel scraper statik HTML alır
   - Çözüm: AutoSupported adapter veya farklı site

3. **User-Agent engeli:**
   - Bazı siteler bot engeller
   - Uygulama otomatik User-Agent header ekler

4. **Encoding sorunu:**
   - Türkçe karakterler bozuk görünüyor
   - Parser ayarlarında `Encoding: UTF-8` deneyin

### 7.2 "403 Forbidden" Hatası

```
❌ HTTP 403 - Access Denied
```

**Çözümler:**
1. Site bot engelliyor olabilir
2. VPN deneyin
3. Rate limiting: Kontrol aralığını artırın

### 7.3 "Connection Timeout" Hatası

```
❌ Connection timed out
```

**Çözümler:**
1. İnternet bağlantısını kontrol edin
2. Site geçici olarak kapalı olabilir
3. Firewall/Antivirus kontrolü

### 7.4 Yanlış Veri Çekilmesi

**Başlık yanlış:**
```xpath
# Belki başka bir element seçiliyor
# Daha spesifik seçici yazın:
❌ .//a
✅ .//h2[@class='title']/a
```

**URL eksik/yanlış:**
```xpath
# href değeri görece olabilir
# Base URL ile birleştirilir: /job/123 → https://site.com/job/123
```

**Boş şehir/şirket:**
```xpath
# Element yok veya farklı class
# F12 ile kontrol edin
# Class adı değişmiş olabilir
```

### 7.5 Debug Modu

Terminal çıktısını takip edin:

```bash
dotnet run --project src/ListingMonitor.UI
```

```
🔄 Eleman.net scraping başlatılıyor...
📥 HTML alındı: 245KB
🔍 Liste seçici: //div[@class='list-items']/div
📊 Bulunan node sayısı: 0      # ← Sorun burada!
⚠️ Hiç ilan bulunamadı
```

---

## 8. Gelişmiş Teknikler

### 8.1 Çoklu Class Seçimi

```xpath
# Birden fazla class'a sahip element
//div[contains(@class,'job') and contains(@class,'item')]

# Class listesinde spesifik değer
//div[contains(concat(' ',@class,' '),' active ')]
```

### 8.2 Sibling Seçimi

```xpath
# Sonraki kardeş
//h2/following-sibling::span

# Önceki kardeş
//span/preceding-sibling::h2
```

### 8.3 Parent Seçimi

```xpath
# Parent element
//a[@class='title']/..

# 2 seviye yukarı
//a[@class='title']/../..
```

### 8.4 Koşullu Seçim

```xpath
# Attribute varsa
//a[@href]

# Attribute yoksa
//a[not(@href)]

# Text içeriyor
//span[contains(text(),'İstanbul')]

# Birden fazla koşul
//div[@class='item' and @data-active='true']
```

### 8.5 Pozisyon Seçimi

```xpath
# İlk element
//div[@class='item'][1]

# Son element
//div[@class='item'][last()]

# 2-5 arası
//div[@class='item'][position() >= 2 and position() <= 5]
```

### 8.6 Attribute Değeri Çekme

```xpath
# href attribute'u
.//a/@href

# data attribute
.//div/@data-id

# Herhangi bir attribute
.//span/@*
```

---

## 📝 Hızlı Referans Kartı

```
┌─────────────────────────────────────────────────────────────┐
│ XPath Hızlı Referans                                        │
├─────────────────────────────────────────────────────────────┤
│ //div                    Tüm div'ler                        │
│ .//div                   Mevcut node altındaki div'ler      │
│ //div[@class='x']        class='x' olan div                 │
│ //div[contains(@c,'x')]  class'ında 'x' geçen div           │
│ //a/@href                Link'in href değeri                │
│ //span/text()            Span'ın metin içeriği              │
│ //div[1]                 İlk div                            │
│ //div[last()]            Son div                            │
│ //div/..                 Div'in parent'ı                    │
│ //div/following-sibling  Div'in sonraki kardeşleri          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ CSS Hızlı Referans                                          │
├─────────────────────────────────────────────────────────────┤
│ .class                   Class seçici                       │
│ #id                      ID seçici                          │
│ div.class                Elemnt + class                     │
│ div > span               Direct child                       │
│ div span                 Descendant                         │
│ [attr='value']           Attribute eşit                     │
│ [attr*='value']          Attribute içerir                   │
│ :first-child             İlk çocuk                          │
│ :nth-child(2)            2. çocuk                           │
└─────────────────────────────────────────────────────────────┘
```

---

## 📞 Yardım

Sorun yaşarsanız:
1. Bu rehberi tekrar okuyun
2. F12 ile DOM yapısını inceleyin
3. [GitHub Issues](https://github.com/gokiceynn/NTPProject/issues)'da soru sorun

---

<div align="center">

**İyi scraping'ler! 🕷️**

</div>
