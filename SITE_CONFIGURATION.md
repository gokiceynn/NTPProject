# ⚙️ Site Konfigürasyon Referansı

<div align="center">

**Tüm site ayarları ve parser konfigürasyonları için teknik referans**

</div>

---

## 📋 İçindekiler

1. [Site Entity Yapısı](#1-site-entity-yapısı)
2. [SiteParserConfig Yapısı](#2-siteparserconfig-yapısı)
3. [Enum Tanımları](#3-enum-tanımları)
4. [Adapter Konfigürasyonları](#4-adapter-konfigürasyonları)
5. [Veritabanı Şeması](#5-veritabanı-şeması)
6. [Örnek Konfigürasyonlar](#6-örnek-konfigürasyonlar)
7. [Validasyon Kuralları](#7-validasyon-kuralları)

---

## 1. Site Entity Yapısı

### 1.1 C# Entity Tanımı

```csharp
public class Site
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;
    
    public SiteType SiteType { get; set; } = SiteType.AutoSupported;
    
    public bool IsActive { get; set; } = true;
    
    public int CheckIntervalMinutes { get; set; } = 10;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public SiteParserConfig? ParserConfig { get; set; }
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
}
```

### 1.2 Alan Açıklamaları

| Alan | Tip | Zorunlu | Varsayılan | Açıklama |
|------|-----|---------|------------|----------|
| `Id` | int | Auto | - | Primary key |
| `Name` | string | ✅ | - | Site adı (max 100 karakter) |
| `BaseUrl` | string | ✅ | - | Scraping URL'i (max 500 karakter) |
| `SiteType` | enum | ✅ | AutoSupported | Site tipi |
| `IsActive` | bool | ✅ | true | Aktif/Pasif durumu |
| `CheckIntervalMinutes` | int | ✅ | 10 | Kontrol aralığı (dakika) |
| `CreatedAt` | DateTime | ✅ | UtcNow | Oluşturulma tarihi |
| `ParserConfig` | object | ❌ | null | Manuel siteler için parser ayarları |

---

## 2. SiteParserConfig Yapısı

### 2.1 C# Entity Tanımı

```csharp
public class SiteParserConfig
{
    public int Id { get; set; }  // FK to Site.Id
    
    [MaxLength(500)]
    public string? ListingItemSelector { get; set; }
    
    [MaxLength(500)]
    public string? TitleSelector { get; set; }
    
    [MaxLength(500)]
    public string? PriceSelector { get; set; }
    
    [MaxLength(500)]
    public string? UrlSelector { get; set; }
    
    [MaxLength(500)]
    public string? DateSelector { get; set; }
    
    [MaxLength(500)]
    public string? ListingIdSelector { get; set; }
    
    [MaxLength(500)]
    public string? CompanySelector { get; set; }
    
    [MaxLength(500)]
    public string? CitySelector { get; set; }
    
    public SelectorType SelectorType { get; set; } = SelectorType.XPath;
    
    [MaxLength(50)]
    public string Encoding { get; set; } = "UTF-8";
    
    // Navigation Property
    public Site? Site { get; set; }
}
```

### 2.2 Alan Açıklamaları

| Alan | Tip | Zorunlu | Açıklama |
|------|-----|---------|----------|
| `Id` | int | ✅ | FK to Sites.Id (1:1 ilişki) |
| `ListingItemSelector` | string | ✅* | İlan listesi container seçicisi |
| `TitleSelector` | string | ✅* | Başlık elementi seçicisi |
| `PriceSelector` | string | ❌ | Fiyat elementi seçicisi |
| `UrlSelector` | string | ✅* | URL elementi seçicisi |
| `DateSelector` | string | ❌ | Tarih elementi seçicisi |
| `ListingIdSelector` | string | ✅* | Unique ID seçicisi |
| `CompanySelector` | string | ❌ | Şirket adı seçicisi |
| `CitySelector` | string | ❌ | Şehir seçicisi |
| `SelectorType` | enum | ✅ | XPath veya CssSelector |
| `Encoding` | string | ✅ | Karakter kodlaması |

> *Manuel siteler için zorunlu

### 2.3 Seçici Örnekleri

**XPath Seçiciler:**
```yaml
ListingItemSelector: //div[contains(@class,'job-list')]/div[@class='job-item']
TitleSelector:       .//h2[@class='title']/a
PriceSelector:       .//span[@class='salary']
UrlSelector:         .//h2[@class='title']/a
DateSelector:        .//span[@class='date']
ListingIdSelector:   .//h2[@class='title']/a/@href
CompanySelector:     .//span[@class='company']
CitySelector:        .//span[@class='location']
```

**CSS Seçiciler:**
```yaml
ListingItemSelector: .job-list .job-item
TitleSelector:       .title a
PriceSelector:       .salary
UrlSelector:         .title a
DateSelector:        .date
ListingIdSelector:   .title a
CompanySelector:     .company
CitySelector:        .location
```

---

## 3. Enum Tanımları

### 3.1 SiteType Enum

```csharp
public enum SiteType
{
    AutoSupported = 0,  // Adapter ile desteklenen
    Manual = 1          // Manuel konfigürasyon gereken
}
```

| Değer | Sayısal | Açıklama |
|-------|---------|----------|
| `AutoSupported` | 0 | Youthall, İlanburda, Microfon gibi özel adapter'ı olan siteler |
| `Manual` | 1 | XPath/CSS seçicilerle konfigüre edilen siteler |

### 3.2 SelectorType Enum

```csharp
public enum SelectorType
{
    XPath = 0,      // XPath seçiciler
    CssSelector = 1 // CSS seçiciler
}
```

| Değer | Sayısal | Açıklama |
|-------|---------|----------|
| `XPath` | 0 | XPath 1.0 sözdizimi |
| `CssSelector` | 1 | CSS seçici sözdizimi |

### 3.3 NotificationStatus Enum

```csharp
public enum NotificationStatus
{
    Pending = 0,   // Beklemede
    Success = 1,   // Başarılı
    Failed = 2     // Başarısız
}
```

---

## 4. Adapter Konfigürasyonları

### 4.1 YouthallAdapter

```csharp
public class YouthallAdapter : ISiteAdapter
{
    public string SiteName => "Youthall";
    public string BaseUrl => "https://youthall.com/tr/talent-programs/";
    
    // REST API Endpoint
    private const string ApiUrl = "https://youthall.com/api/talent-programs";
}
```

**Özellikler:**
- REST API kullanır
- JSON response parse eder
- Parser config gerekmez

**Desteklenen Alanlar:**
| Alan | Kaynak |
|------|--------|
| Title | `program.title` |
| Company | `program.company.name` |
| Url | `program.slug` → URL oluşturulur |
| City | `program.location` |
| ExternalId | `program.id` |

### 4.2 IlanburdaAdapter

```csharp
public class IlanburdaAdapter : ISiteAdapter
{
    public string SiteName => "İlanburda";
    public string BaseUrl => "https://ilanburda.net/8/is-ilanlari";
}
```

**Özellikler:**
- HTML parsing kullanır
- User-Agent header gerekli
- Table row yapısı

**Seçiciler (Hardcoded):**
```xpath
Liste:    //tr[@class='satir_link']
Başlık:   .//a[@class='joblisting']
URL:      .//a[@class='joblisting']/@href
Şehir:    .//td[3]
Tarih:    .//td[4]
```

### 4.3 MicrofonAdapter

```csharp
public class MicrofonAdapter : ISiteAdapter
{
    public string SiteName => "Microfon";
    public string BaseUrl => "https://microfon.co/scholarship";
}
```

**Özellikler:**
- Next.js sitesi
- `__NEXT_DATA__` JSON'dan parse eder
- Fallback: DOM parsing

**JSON Yapısı:**
```json
{
  "props": {
    "pageProps": {
      "scholarships": [
        {
          "id": "...",
          "title": "...",
          "organization": "...",
          "url": "..."
        }
      ]
    }
  }
}
```

---

## 5. Veritabanı Şeması

### 5.1 Sites Tablosu

```sql
CREATE TABLE Sites (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    BaseUrl TEXT NOT NULL,
    SiteType INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CheckIntervalMinutes INTEGER NOT NULL DEFAULT 10,
    CreatedAt TEXT NOT NULL
);

-- Index
CREATE INDEX IX_Sites_IsActive ON Sites(IsActive);
CREATE INDEX IX_Sites_SiteType ON Sites(SiteType);
```

### 5.2 SiteParserConfigs Tablosu

```sql
CREATE TABLE SiteParserConfigs (
    Id INTEGER PRIMARY KEY,
    ListingItemSelector TEXT,
    TitleSelector TEXT,
    PriceSelector TEXT,
    UrlSelector TEXT,
    DateSelector TEXT,
    ListingIdSelector TEXT,
    CompanySelector TEXT,
    CitySelector TEXT,
    SelectorType INTEGER NOT NULL DEFAULT 0,
    Encoding TEXT DEFAULT 'UTF-8',
    FOREIGN KEY (Id) REFERENCES Sites(Id) ON DELETE CASCADE
);
```

### 5.3 İlişki Diyagramı

```
┌─────────────┐     1:1     ┌──────────────────┐
│   Sites     │─────────────│ SiteParserConfig │
├─────────────┤             ├──────────────────┤
│ Id (PK)     │◄────────────│ Id (PK, FK)      │
│ Name        │             │ ListingItem...   │
│ BaseUrl     │             │ TitleSelector    │
│ SiteType    │             │ ...              │
│ IsActive    │             └──────────────────┘
│ CheckInt... │
│ CreatedAt   │
└──────┬──────┘
       │
       │ 1:N
       ▼
┌─────────────┐
│  Listings   │
├─────────────┤
│ Id (PK)     │
│ SiteId (FK) │
│ Title       │
│ ...         │
└─────────────┘
```

---

## 6. Örnek Konfigürasyonlar

### 6.1 Youthall (AutoSupported)

```json
{
  "Site": {
    "Name": "Youthall",
    "BaseUrl": "https://youthall.com/tr/talent-programs/",
    "SiteType": 0,
    "IsActive": true,
    "CheckIntervalMinutes": 10
  },
  "ParserConfig": null
}
```

### 6.2 İlanburda (AutoSupported)

```json
{
  "Site": {
    "Name": "İlanburda",
    "BaseUrl": "https://ilanburda.net/8/is-ilanlari",
    "SiteType": 0,
    "IsActive": true,
    "CheckIntervalMinutes": 10
  },
  "ParserConfig": null
}
```

### 6.3 Eleman.net (Manual - XPath) ✅ Test Edildi

```json
{
  "Site": {
    "Name": "Eleman.net",
    "BaseUrl": "https://www.eleman.net/is-ilanlari",
    "SiteType": 1,
    "IsActive": true,
    "CheckIntervalMinutes": 15
  },
  "ParserConfig": {
    "ListingItemSelector": "//div[contains(@class,'ilan_listeleme_bol')]",
    "TitleSelector": ".//h3[contains(@class,'c-showcase-box__title')]",
    "PriceSelector": null,
    "UrlSelector": ".//a",
    "DateSelector": null,
    "ListingIdSelector": ".//a",
    "CompanySelector": null,
    "CitySelector": null,
    "SelectorType": 0,
    "Encoding": "UTF-8"
  }
}
```

### 6.4 GenericSite (Manual - CSS)

```json
{
  "Site": {
    "Name": "GenericSite",
    "BaseUrl": "https://example.com/jobs",
    "SiteType": 1,
    "IsActive": true,
    "CheckIntervalMinutes": 30
  },
  "ParserConfig": {
    "ListingItemSelector": ".job-list .job-card",
    "TitleSelector": ".title a",
    "PriceSelector": ".salary",
    "UrlSelector": ".title a",
    "DateSelector": ".date",
    "ListingIdSelector": ".title a",
    "CompanySelector": ".company",
    "CitySelector": ".location",
    "SelectorType": 1,
    "Encoding": "UTF-8"
  }
}
```

---

## 7. Validasyon Kuralları

### 7.1 Site Validasyonu

```csharp
public class SiteValidator
{
    public ValidationResult Validate(Site site)
    {
        var errors = new List<string>();
        
        // Name validasyonu
        if (string.IsNullOrWhiteSpace(site.Name))
            errors.Add("Site adı zorunludur");
        if (site.Name?.Length > 100)
            errors.Add("Site adı 100 karakterden uzun olamaz");
            
        // URL validasyonu
        if (string.IsNullOrWhiteSpace(site.BaseUrl))
            errors.Add("Base URL zorunludur");
        if (!Uri.TryCreate(site.BaseUrl, UriKind.Absolute, out _))
            errors.Add("Geçersiz URL formatı");
            
        // Interval validasyonu
        if (site.CheckIntervalMinutes < 1)
            errors.Add("Kontrol aralığı en az 1 dakika olmalı");
        if (site.CheckIntervalMinutes > 1440)
            errors.Add("Kontrol aralığı en fazla 1440 dakika (24 saat) olabilir");
            
        // Manuel site için ParserConfig kontrolü
        if (site.SiteType == SiteType.Manual && site.ParserConfig == null)
            errors.Add("Manuel siteler için parser konfigürasyonu zorunludur");
            
        return new ValidationResult(errors);
    }
}
```

### 7.2 ParserConfig Validasyonu

```csharp
public class ParserConfigValidator
{
    public ValidationResult Validate(SiteParserConfig config)
    {
        var errors = new List<string>();
        
        // Zorunlu seçiciler
        if (string.IsNullOrWhiteSpace(config.ListingItemSelector))
            errors.Add("Liste seçici zorunludur");
        if (string.IsNullOrWhiteSpace(config.TitleSelector))
            errors.Add("Başlık seçici zorunludur");
        if (string.IsNullOrWhiteSpace(config.UrlSelector))
            errors.Add("URL seçici zorunludur");
        if (string.IsNullOrWhiteSpace(config.ListingIdSelector))
            errors.Add("ID seçici zorunludur");
            
        // XPath sözdizimi kontrolü (basit)
        if (config.SelectorType == SelectorType.XPath)
        {
            if (config.ListingItemSelector?.StartsWith("//") == false 
                && config.ListingItemSelector?.StartsWith(".//") == false)
            {
                errors.Add("XPath seçici // veya .// ile başlamalı");
            }
        }
        
        return new ValidationResult(errors);
    }
}
```

### 7.3 Seçici Sözdizimi Örnekleri

**Geçerli XPath:**
```xpath
✅ //div[@class='item']
✅ .//h2/a
✅ //table//tr[position() > 1]
✅ //a[contains(@href,'job')]/@href
```

**Geçersiz XPath:**
```xpath
❌ div.item          # CSS sözdizimi
❌ /div              # Root'tan başlıyor
❌ @href             # Attribute tek başına
```

**Geçerli CSS:**
```css
✅ .job-list .item
✅ #main-content a
✅ div.card > h2
✅ [data-id='123']
```

---

## 📊 Özet Tablo

| Site Tipi | Parser Config | Adapter | Seçici Tipi |
|-----------|---------------|---------|-------------|
| AutoSupported | ❌ Gerekli değil | ✅ Gerekli | - |
| Manual | ✅ Gerekli | ❌ Kullanılmaz | XPath/CSS |

| Seçici | Manuel Site için | Açıklama |
|--------|------------------|----------|
| ListingItemSelector | ✅ Zorunlu | Her ilan kartı |
| TitleSelector | ✅ Zorunlu | İlan başlığı |
| UrlSelector | ✅ Zorunlu | İlan linki |
| ListingIdSelector | ✅ Zorunlu | Unique ID |
| CompanySelector | ❌ Opsiyonel | Şirket adı |
| CitySelector | ❌ Opsiyonel | Şehir |
| DateSelector | ❌ Opsiyonel | Tarih |
| PriceSelector | ❌ Opsiyonel | Maaş/Fiyat |

---

<div align="center">

**Teknik sorular için [GitHub Issues](https://github.com/gokiceynn/NTPProject/issues) kullanın 🔧**

</div>
