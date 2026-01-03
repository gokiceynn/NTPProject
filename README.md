# 🎯 İlan Takip Sistemi (Listing Monitor)

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![Avalonia](https://img.shields.io/badge/Avalonia-11.0-8B5CF6?style=for-the-badge)
![SQLite](https://img.shields.io/badge/SQLite-3-003B57?style=for-the-badge&logo=sqlite)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**Çoklu web sitelerinden ilanları otomatik takip eden, alarm kurallarıyla filtreleyip email bildirimi gönderen masaüstü uygulaması.**

[Özellikler](#-özellikler) • [Kurulum](#-kurulum) • [Kullanım](#-kullanım) • [Mimari](#-mimari) • [API](#-api-referansı)

</div>

---

## 📋 İçindekiler

- [Özellikler](#-özellikler)
- [Ekran Görüntüleri](#-ekran-görüntüleri)
- [Kurulum](#-kurulum)
- [Kullanım](#-kullanım)
- [Desteklenen Siteler](#-desteklenen-siteler)
- [Mimari](#-mimari)
- [Veritabanı Şeması](#-veritabanı-şeması)
- [API Referansı](#-api-referansı)
- [Konfigürasyon](#-konfigürasyon)
- [Geliştirici Rehberi](#-geliştirici-rehberi)
- [Sorun Giderme](#-sorun-giderme)
- [Katkıda Bulunma](#-katkıda-bulunma)

---

## ✨ Özellikler

### 🌐 Site Yönetimi
- **Otomatik Desteklenen Siteler**: Youthall, İlanburda (adapter ile)
- **Manuel Site Ekleme**: XPath/CSS seçicilerle özel site tanımlama
- **Site Durumu**: Aktif/Pasif geçişi
- **Scraping**: Manuel veya zamanlı otomatik scraping

### 🔔 Alarm Kuralları
- **Anahtar Kelime Filtresi**: Tam kelime eşleştirmesi (Regex word boundary)
- **Şehir Filtresi**: İlan şehrine göre filtreleme
- **Fiyat Aralığı**: Min/Max fiyat filtresi
- **Site Filtresi**: Belirli siteye özel kurallar
- **Kural Testi**: Kaydetmeden önce eşleşen ilanları görme

### 📧 Email Bildirimleri
- **Anlık Bildirim**: Yeni ilan bulunduğunda hemen
- **Zamanlanmış Email**: 1/6/12/24 saat aralıklarla toplu
- **Manuel Gönderim**: Seçili siteden tüm ilanları gönder
- **HTML Formatı**: Güzel tasarımlı email şablonları

### 🎨 Kullanıcı Arayüzü
- **Modern Tasarım**: Dark/Light tema desteği
- **Responsive**: Her ekran boyutuna uyumlu
- **Tab Bazlı**: Dashboard, Siteler, Kurallar, İlanlar, Ayarlar, Loglar
- **Doğrudan Aksiyonlar**: Her öğenin yanında işlem butonları

### 💾 Veri Yönetimi
- **SQLite Veritabanı**: Yerel, hızlı, portable
- **Backup/Restore**: Yedekleme ve geri yükleme
- **Otomatik Migration**: Uygulama başlangıcında şema güncelleme

---

## 📸 Ekran Görüntüleri

### Dashboard
```
┌─────────────────────────────────────────────────────────────┐
│  🎯 İlan Takip Sistemi                    [▶️ Başlat] [⚙️]  │
├─────────────────────────────────────────────────────────────┤
│  📊 Dashboard │ 🌐 Siteler │ 🔔 Kurallar │ 📋 İlanlar │ ⚙️  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐    │
│  │ 4        │  │ 1105     │  │ 3        │  │ 24       │    │
│  │ Siteler  │  │ İlanlar  │  │ Kurallar │  │ Bugün    │    │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘    │
│                                                             │
│  📈 Son Aktiviteler                                         │
│  ├─ ✅ Youthall: 320 ilan çekildi                          │
│  ├─ ✅ İlanburda: 450 ilan çekildi                         │
│  └─ 📧 2 bildirim gönderildi                               │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Siteler Sekmesi
```
┌─────────────────────────────────────────────────────────────┐
│  Site Yönetimi                    [➕ Site Ekle] [🔄 Yenile]│
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 🌐 Youthall                    ● Aktif              │   │
│  │    https://youthall.com/tr/talent-programs/         │   │
│  │    🏷️ AutoSupported  ⏱️ 10 dk                       │   │
│  │                              [✏️][🔄][🔀][🗑️]      │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 🌐 İlanburda                   ● Aktif              │   │
│  │    https://ilanburda.net/8/is-ilanlari              │   │
│  │    🏷️ AutoSupported  ⏱️ 10 dk                       │   │
│  │                              [✏️][🔄][🔀][🗑️]      │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Kurallar Sekmesi
```
┌─────────────────────────────────────────────────────────────┐
│  Alarm Kuralları                 [➕ Kural Ekle] [🔄 Yenile]│
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 🔔 Burs Bildirimleri                    ● Aktif     │   │
│  │    burs, scholarship, bursiyer                      │   │
│  │    📍 Tüm Siteler  🏙️ İstanbul  📧 email@test.com  │   │
│  │    ⏱️ 6h  🆕 Yeni                                   │   │
│  │                        [✏️][🎯][📧][🔄][🗑️]        │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─ 🎯 Kurala Uyan İlanlar ─────────────────────────────┐  │
│  │  ✅ Burs Bildirimleri analiz tamamlandı             │  │
│  │  📊 Toplam 1105 ilan incelendi                      │  │
│  │  🎯 5 ilan kurala uyuyor                            │  │
│  │                                                      │  │
│  │  • Altuğ Fonu Bursu - İstanbul                      │  │
│  │  • Koç Bursu 2025 - İstanbul                        │  │
│  │                              [📧 Test Mail Gönder]   │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🚀 Kurulum

### Gereksinimler
- .NET 8.0 SDK
- macOS / Windows / Linux

### Adımlar

```bash
# 1. Repoyu klonla
git clone https://github.com/gokiceynn/NTPProject.git
cd NTPProject

# 2. Bağımlılıkları yükle
dotnet restore

# 3. Build al
dotnet build src/ListingMonitor.UI

# 4. Çalıştır
dotnet run --project src/ListingMonitor.UI
```

### İlk Çalıştırma
Uygulama ilk açıldığında:
1. ✅ SQLite veritabanı otomatik oluşturulur
2. ✅ Youthall ve İlanburda siteleri otomatik eklenir
3. ✅ SMTP ayarları yapılandırılmayı bekler

---

## 📖 Kullanım

### 1. SMTP Ayarları (İlk Yapılması Gereken)

```
Ayarlar → SMTP Ayarlarını Düzenle

┌─────────────────────────────────────┐
│ SMTP Host:     smtp.gmail.com       │
│ Port:          587                  │
│ Username:      your@gmail.com       │
│ Password:      app-password         │
│ From Email:    your@gmail.com       │
│ StartTLS:      ✅                   │
└─────────────────────────────────────┘
```

> **Gmail için**: 2FA aktif olmalı, "Uygulama Şifreleri"nden yeni şifre oluşturun.

### 2. Site Ekleme

#### Otomatik Desteklenen Site
```
Siteler → ➕ Site Ekle

Site Adı:    Youthall
URL:         https://youthall.com/tr/talent-programs/
Site Tipi:   AutoSupported
```

#### Manuel Site (XPath ile)
```
Siteler → ➕ Site Ekle

Site Adı:       Eleman.net
URL:            https://www.eleman.net/is-ilanlari
Site Tipi:      Manual
Seçici Tipi:    XPath

Parser Ayarları:
├─ Liste Seçici:    //div[@class='list-items']/div
├─ Başlık Seçici:   .//h2/a
├─ URL Seçici:      .//h2/a
├─ ID Seçici:       .//h2/a/@href
├─ Şirket Seçici:   .//span[@class='company']
└─ Tarih Seçici:    .//span[@class='date']
```

### 3. Alarm Kuralı Oluşturma

```
Kurallar → ➕ Kural Ekle

┌─────────────────────────────────────────────┐
│ 📋 Temel Bilgiler                           │
│ Kural Adı:     Burs Bildirimleri            │
│ Hedef Site:    🌐 Tüm Siteler               │
│ ✅ Aktif       ✅ Sadece Yeni İlanlar       │
├─────────────────────────────────────────────┤
│ 🔍 Filtre Kriterleri                        │
│ Anahtar Kelimeler: burs, scholarship        │
│ Şehir:             İstanbul                 │
│ Min Fiyat:         0                        │
│ Max Fiyat:         -                        │
├─────────────────────────────────────────────┤
│ 📧 Bildirim Ayarları                        │
│ Email: your@email.com                       │
│ ✅ Zamanlanmış Email: Her 6 saat            │
└─────────────────────────────────────────────┘

[🧪 Kuralı Test Et]  →  5 ilan eşleşti
[💾 Kaydet]
```

### 4. Scraping Başlatma

**Manuel Scraping:**
```
Siteler → [Site Satırı] → 🔄 butonu
```

**Otomatik Scraping:**
```
Header → ▶️ Başlat
```
> Tüm aktif siteleri belirtilen aralıklarla kontrol eder.

### 5. İlan Mail Gönderme

```
Ayarlar → İlan Mail Gönder

Site Seçimi:    📊 Tüm Siteler / 🌐 Youthall
Alıcı Email:    your@email.com

[📧 İlanları Mail Gönder]
```

---

## 🌐 Desteklenen Siteler

### Otomatik Desteklenen (AutoSupported)

| Site | URL | Adapter | Durum |
|------|-----|---------|-------|
| Youthall | youthall.com | `YouthallAdapter` | ✅ Aktif |
| İlanburda | ilanburda.net | `IlanburdaAdapter` | ✅ Aktif |
| Microfon | microfon.co | `MicrofonAdapter` | ✅ Aktif |

### Manuel Eklenebilir

| Site | Yöntem | Seçici Tipi |
|------|--------|-------------|
| Eleman.net | Manuel | XPath |
| Kariyer.net | Manuel | XPath |
| Indeed | Manuel | CSS |
| LinkedIn | Manuel | XPath |

> Manuel site ekleme için bkz: [SITE_EKLEME_REHBERI.md](SITE_EKLEME_REHBERI.md)

---

## 🏗 Mimari

### Clean Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        UI Layer                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Avalonia MVVM                                       │    │
│  │  ├── Views (AXAML)                                   │    │
│  │  ├── ViewModels (ObservableObject)                   │    │
│  │  └── Converters                                      │    │
│  └─────────────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                         │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Services                                            │    │
│  │  ├── SiteService                                     │    │
│  │  ├── AlertRuleService                                │    │
│  │  ├── NotificationService                             │    │
│  │  ├── ScraperSchedulerService                         │    │
│  │  ├── ListingDiffService                              │    │
│  │  ├── InitialRunEmailService                          │    │
│  │  └── DatabaseBackupService                           │    │
│  └─────────────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                       │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Data                                                │    │
│  │  ├── AppDbContext (EF Core)                          │    │
│  │  └── SQLite Database                                 │    │
│  │                                                      │    │
│  │  Scraping                                            │    │
│  │  ├── ModernScrapingService                           │    │
│  │  ├── ManualSiteScraper                               │    │
│  │  └── Adapters/                                       │    │
│  │      ├── YouthallAdapter                             │    │
│  │      ├── IlanburdaAdapter                            │    │
│  │      └── MicrofonAdapter                             │    │
│  │                                                      │    │
│  │  Email                                               │    │
│  │  └── SmtpEmailService (MailKit)                      │    │
│  └─────────────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                            │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Entities                                            │    │
│  │  ├── Site                                            │    │
│  │  ├── SiteParserConfig                                │    │
│  │  ├── Listing                                         │    │
│  │  ├── AlertRule                                       │    │
│  │  ├── NotificationLog                                 │    │
│  │  └── AppSetting                                      │    │
│  │                                                      │    │
│  │  Enums                                               │    │
│  │  ├── SiteType (AutoSupported, Manual)                │    │
│  │  ├── SelectorType (XPath, CssSelector)               │    │
│  │  └── NotificationStatus                              │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### Proje Yapısı

```
ListingMonitor/
├── src/
│   ├── ListingMonitor.Domain/           # Entity ve Enum tanımları
│   │   ├── Entities/
│   │   │   ├── Site.cs
│   │   │   ├── SiteParserConfig.cs
│   │   │   ├── Listing.cs
│   │   │   ├── AlertRule.cs
│   │   │   ├── NotificationLog.cs
│   │   │   └── AppSetting.cs
│   │   └── Enums/
│   │       └── Enums.cs
│   │
│   ├── ListingMonitor.Application/      # İş mantığı servisleri
│   │   └── Services/
│   │       ├── SiteService.cs
│   │       ├── AlertRuleService.cs
│   │       ├── NotificationService.cs
│   │       ├── ScraperSchedulerService.cs
│   │       ├── ListingDiffService.cs
│   │       ├── InitialRunEmailService.cs
│   │       └── DatabaseBackupService.cs
│   │
│   ├── ListingMonitor.Infrastructure/   # Harici servisler
│   │   ├── Data/
│   │   │   └── AppDbContext.cs
│   │   ├── Email/
│   │   │   ├── IEmailService.cs
│   │   │   ├── SmtpEmailService.cs
│   │   │   └── SmtpSettings.cs
│   │   └── Scraping/
│   │       ├── ISiteScraper.cs
│   │       ├── ModernScrapingService.cs
│   │       ├── ManualSiteScraper.cs
│   │       └── Adapters/
│   │           ├── ISiteAdapter.cs
│   │           ├── YouthallAdapter.cs
│   │           ├── IlanburdaAdapter.cs
│   │           └── MicrofonAdapter.cs
│   │
│   └── ListingMonitor.UI/               # Avalonia UI
│       ├── Views/
│       │   ├── MainWindow.axaml
│       │   ├── SiteEditWindow.axaml
│       │   ├── AlertRuleEditWindow.axaml
│       │   └── SmtpSettingsWindow.axaml
│       ├── ViewModels/
│       │   ├── MainWindowViewModel.cs
│       │   ├── SiteEditViewModel.cs
│       │   ├── AlertRuleEditViewModel.cs
│       │   └── SmtpSettingsViewModel.cs
│       └── Program.cs
│
├── README.md
├── KULLANIM_KILAVUZU.md
├── SITE_EKLEME_REHBERI.md
└── SITE_CONFIGURATION.md
```

---

## 🗄 Veritabanı Şeması

### Entity-Relationship Diagram

```
┌─────────────────┐       ┌─────────────────┐
│     Sites       │       │ SiteParserConfig│
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │──────<│ Id (PK, FK)     │
│ Name            │       │ ListingItem...  │
│ BaseUrl         │       │ TitleSelector   │
│ SiteType        │       │ PriceSelector   │
│ IsActive        │       │ UrlSelector     │
│ CheckInterval   │       │ SelectorType    │
│ CreatedAt       │       │ Encoding        │
└────────┬────────┘       └─────────────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐       ┌─────────────────┐
│    Listings     │       │   AlertRules    │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ SiteId (FK)     │       │ SiteId (FK)?    │
│ ExternalId      │       │ Name            │
│ Title           │       │ Keywords        │
│ Company         │       │ MinPrice        │
│ Price           │       │ MaxPrice        │
│ Url             │       │ City            │
│ City            │       │ EmailsToNotify  │
│ FirstSeenAt     │       │ IsActive        │
│ CreatedAtOnSite │       │ OnlyNewListings │
└────────┬────────┘       │ EnableScheduled │
         │                │ EmailInterval   │
         │ 1:N            │ NextEmailSendAt │
         ▼                │ CreatedAt       │
┌─────────────────┐       └────────┬────────┘
│NotificationLogs │                │
├─────────────────┤                │ 1:N
│ Id (PK)         │                ▼
│ RuleId (FK)?    │       ┌─────────────────┐
│ ListingId (FK)? │       │  NotificationLog│
│ ToEmail         │       │  (via RuleId)   │
│ Status          │       └─────────────────┘
│ ErrorMessage    │
│ SentAt          │
└─────────────────┘

┌─────────────────┐
│   AppSettings   │
├─────────────────┤
│ Id (PK)         │
│ Key (Unique)    │
│ Value           │
└─────────────────┘
```

### SQL Şeması

```sql
-- Sites tablosu
CREATE TABLE Sites (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    BaseUrl TEXT NOT NULL,
    SiteType INTEGER NOT NULL DEFAULT 0,  -- 0: AutoSupported, 1: Manual
    IsActive INTEGER NOT NULL DEFAULT 1,
    CheckIntervalMinutes INTEGER NOT NULL DEFAULT 10,
    CreatedAt TEXT NOT NULL
);

-- SiteParserConfigs tablosu
CREATE TABLE SiteParserConfigs (
    Id INTEGER PRIMARY KEY,  -- FK to Sites.Id
    ListingItemSelector TEXT,
    TitleSelector TEXT,
    PriceSelector TEXT,
    UrlSelector TEXT,
    DateSelector TEXT,
    ListingIdSelector TEXT,
    CompanySelector TEXT,
    CitySelector TEXT,
    SelectorType INTEGER NOT NULL DEFAULT 0,  -- 0: XPath, 1: CSS
    Encoding TEXT DEFAULT 'UTF-8',
    FOREIGN KEY (Id) REFERENCES Sites(Id) ON DELETE CASCADE
);

-- Listings tablosu
CREATE TABLE Listings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SiteId INTEGER NOT NULL,
    ExternalId TEXT NOT NULL,
    Title TEXT NOT NULL,
    Company TEXT,
    Price REAL,
    Url TEXT NOT NULL,
    City TEXT,
    FirstSeenAt TEXT NOT NULL,
    CreatedAtOnSite TEXT,
    FOREIGN KEY (SiteId) REFERENCES Sites(Id) ON DELETE CASCADE,
    UNIQUE(SiteId, ExternalId)
);

-- AlertRules tablosu
CREATE TABLE AlertRules (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    SiteId INTEGER,  -- NULL = tüm siteler
    Keywords TEXT,
    MinPrice REAL,
    MaxPrice REAL,
    City TEXT,
    EmailsToNotify TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    OnlyNewListings INTEGER NOT NULL DEFAULT 1,
    EnableScheduledEmail INTEGER NOT NULL DEFAULT 0,
    EmailIntervalHours INTEGER,
    NextEmailSendAt TEXT,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (SiteId) REFERENCES Sites(Id) ON DELETE SET NULL
);

-- NotificationLogs tablosu
CREATE TABLE NotificationLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RuleId INTEGER,
    ListingId INTEGER,
    ToEmail TEXT NOT NULL,
    Status INTEGER NOT NULL,  -- 0: Pending, 1: Success, 2: Failed
    ErrorMessage TEXT,
    SentAt TEXT NOT NULL,
    FOREIGN KEY (RuleId) REFERENCES AlertRules(Id) ON DELETE SET NULL,
    FOREIGN KEY (ListingId) REFERENCES Listings(Id) ON DELETE SET NULL
);

-- AppSettings tablosu
CREATE TABLE AppSettings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Key TEXT NOT NULL UNIQUE,
    Value TEXT
);
```

---

## 📚 API Referansı

### SiteService

```csharp
public class SiteService
{
    // Tüm siteleri getir
    Task<IEnumerable<Site>> GetAllSitesAsync();
    
    // Site ekle
    Task AddSiteAsync(Site site);
    
    // Site güncelle
    Task UpdateSiteAsync(Site site);
    
    // Site sil
    Task DeleteSiteAsync(int siteId);
    
    // Site scrape et
    Task<List<ListingDto>> ScrapeSiteAsync(int siteId);
}
```

### AlertRuleService

```csharp
public class AlertRuleService
{
    // Tüm kuralları getir
    Task<IEnumerable<AlertRule>> GetAllRulesAsync();
    
    // Kural ekle
    Task AddRuleAsync(AlertRule rule);
    
    // Kural güncelle
    Task UpdateRuleAsync(AlertRule rule);
    
    // Kural sil
    Task DeleteRuleAsync(int ruleId);
    
    // İlan kurala uyuyor mu?
    bool DoesListingMatchRule(Listing listing, AlertRule rule);
}
```

### ISiteScraper

```csharp
public interface ISiteScraper
{
    // İlanları çek
    Task<IEnumerable<ListingDto>> FetchListingsAsync(Site site, SiteParserConfig? config);
    
    // Adapter'ı test et
    Task<bool> TestAdapterAsync(string siteName);
}
```

### ISiteAdapter

```csharp
public interface ISiteAdapter : IDisposable
{
    string SiteName { get; }
    string BaseUrl { get; }
    
    // İlanları çek
    Task<IEnumerable<ListingDto>> ScrapeListingsAsync();
    
    // Site erişilebilir mi?
    Task<bool> IsAvailableAsync();
}
```

---

## ⚙️ Konfigürasyon

### SMTP Ayarları

| Ayar | Açıklama | Örnek |
|------|----------|-------|
| SmtpHost | SMTP sunucu adresi | smtp.gmail.com |
| SmtpPort | Port numarası | 587 |
| UseStartTls | TLS kullanımı | true |
| SmtpUsername | Kullanıcı adı | user@gmail.com |
| SmtpPassword | Şifre (App Password) | xxxx-xxxx-xxxx |
| FromEmail | Gönderen email | user@gmail.com |

### Site Parser Konfigürasyonu (Manuel Siteler)

| Alan | Açıklama | Örnek XPath |
|------|----------|-------------|
| ListingItemSelector | İlan listesi container'ı | `//div[@class='job-item']` |
| TitleSelector | Başlık elementi | `.//h2/a` |
| UrlSelector | Link elementi | `.//h2/a` |
| PriceSelector | Fiyat elementi | `.//span[@class='price']` |
| CompanySelector | Şirket adı | `.//span[@class='company']` |
| CitySelector | Şehir | `.//span[@class='location']` |
| ListingIdSelector | Unique ID | `.//h2/a/@href` |

### Tema Ayarları

| Tema | Arka Plan | Kart | Yazı |
|------|-----------|------|------|
| Dark | #0F172A | #1E293B | #F1F5F9 |
| Light | #F1F5F9 | #FFFFFF | #1E293B |

---

## 👨‍💻 Geliştirici Rehberi

### Yeni Adapter Ekleme

1. **Adapter sınıfı oluştur:**

```csharp
// Infrastructure/Scraping/Adapters/NewSiteAdapter.cs
public class NewSiteAdapter : ISiteAdapter
{
    public string SiteName => "NewSite";
    public string BaseUrl => "https://newsite.com";
    
    private readonly HttpClient _httpClient;
    
    public NewSiteAdapter(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<IEnumerable<ListingDto>> ScrapeListingsAsync()
    {
        var html = await _httpClient.GetStringAsync(BaseUrl);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        var listings = new List<ListingDto>();
        var nodes = doc.DocumentNode.SelectNodes("//div[@class='listing']");
        
        foreach (var node in nodes ?? Enumerable.Empty<HtmlNode>())
        {
            listings.Add(new ListingDto
            {
                Title = node.SelectSingleNode(".//h2")?.InnerText?.Trim(),
                Url = node.SelectSingleNode(".//a")?.GetAttributeValue("href", ""),
                // ... diğer alanlar
            });
        }
        
        return listings;
    }
    
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
    
    public void Dispose() { }
}
```

2. **ModernScrapingService'e ekle:**

```csharp
// ModernScrapingService.cs constructor
_newSiteAdapter = new NewSiteAdapter(httpClient);

// FetchListingsAsync metodunda
if (site.Name.Contains("NewSite", StringComparison.OrdinalIgnoreCase))
{
    return await _newSiteAdapter.ScrapeListingsAsync();
}
```

3. **Program.cs'e default site olarak ekle (opsiyonel):**

```csharp
var defaultSites = new[]
{
    new { Name = "NewSite", Url = "https://newsite.com/listings" }
};
```

### Build & Publish

```bash
# Debug build
dotnet build src/ListingMonitor.UI

# Release build
dotnet build src/ListingMonitor.UI -c Release

# Self-contained publish (macOS)
dotnet publish src/ListingMonitor.UI -c Release -r osx-x64 --self-contained

# Self-contained publish (Windows)
dotnet publish src/ListingMonitor.UI -c Release -r win-x64 --self-contained
```

---

## 🔧 Sorun Giderme

### Sık Karşılaşılan Sorunlar

#### 1. "SMTP Bağlantı Hatası"
```
Çözüm:
- Gmail kullanıyorsanız 2FA aktif olmalı
- "Uygulama Şifreleri"nden yeni şifre oluşturun
- Port 587, StartTLS aktif olmalı
```

#### 2. "403 Forbidden" Scraping Hatası
```
Çözüm:
- User-Agent header'ı otomatik eklenir
- Bazı siteler bot engelleyebilir, manuel scraping deneyin
```

#### 3. "Eklenen Site İlan Bulamıyor"
```
Çözüm:
1. Tarayıcıda siteyi açın
2. F12 → Elements ile DOM yapısını inceleyin
3. XPath seçicilerini kontrol edin
4. Site dinamik JS kullanıyorsa çalışmayabilir
```

#### 4. "Veritabanı Tabloları Yok"
```
Çözüm:
- Uygulama ilk açılışta tabloları oluşturur
- DB dosyasını silip yeniden başlatın
```

### Log Takibi

Terminal çıktısında tüm işlemler loglanır:
```
✅ Veritabanı tabloları hazır
🌐 Youthall sitesi ekleniyor...
✅ Youthall sitesi eklendi
🔄 Youthall scraping başlatılıyor...
✅ 320 ilan bulundu
📧 2 bildirim gönderildi
```

---

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'feat: Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

### Commit Formatı
```
feat: Yeni özellik
fix: Bug düzeltme
docs: Dokümantasyon
style: Kod formatı
refactor: Refactoring
test: Test ekleme
chore: Bakım işleri
```

---

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

---

## 📞 İletişim

- **GitHub**: [github.com/gokiceynn/NTPProject](https://github.com/gokiceynn/NTPProject)
- **GitHub Issues**: Bug raporları ve özellik istekleri için

---

<div align="center">

**⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!**

Made with ❤️ using .NET and Avalonia

</div>
