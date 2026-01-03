# 📚 API Referansı

<div align="center">

**Tüm servisler, interface'ler ve metodlar için teknik referans**

</div>

---

## 📋 İçindekiler

1. [Domain Layer](#1-domain-layer)
2. [Application Layer](#2-application-layer)
3. [Infrastructure Layer](#3-infrastructure-layer)
4. [UI Layer](#4-ui-layer)
5. [DTO'lar](#5-dtolar)
6. [Dependency Injection](#6-dependency-injection)
7. [Event'ler ve Callback'ler](#7-eventler-ve-callbackler)

---

## 1. Domain Layer

### 1.1 Entities

#### Site

```csharp
namespace ListingMonitor.Domain.Entities
{
    public class Site
    {
        // Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public SiteType SiteType { get; set; }
        public bool IsActive { get; set; }
        public int CheckIntervalMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation
        public SiteParserConfig? ParserConfig { get; set; }
        public ICollection<Listing> Listings { get; set; }
        public ICollection<AlertRule> AlertRules { get; set; }
    }
}
```

#### Listing

```csharp
namespace ListingMonitor.Domain.Entities
{
    public class Listing
    {
        // Properties
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string ExternalId { get; set; }
        public string Title { get; set; }
        public string? Company { get; set; }
        public decimal? Price { get; set; }
        public string Url { get; set; }
        public string? City { get; set; }
        public DateTime FirstSeenAt { get; set; }
        public DateTime? CreatedAtOnSite { get; set; }
        
        // Navigation
        public Site? Site { get; set; }
        public ICollection<NotificationLog> NotificationLogs { get; set; }
    }
}
```

#### AlertRule

```csharp
namespace ListingMonitor.Domain.Entities
{
    public class AlertRule
    {
        // Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public int? SiteId { get; set; }  // null = tüm siteler
        public string? Keywords { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? City { get; set; }
        public string EmailsToNotify { get; set; }
        public bool IsActive { get; set; }
        public bool OnlyNewListings { get; set; }
        public bool EnableScheduledEmail { get; set; }
        public int? EmailIntervalHours { get; set; }
        public DateTime? NextEmailSendAt { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation
        public Site? Site { get; set; }
        public ICollection<NotificationLog> NotificationLogs { get; set; }
    }
}
```

#### SiteParserConfig

```csharp
namespace ListingMonitor.Domain.Entities
{
    public class SiteParserConfig
    {
        // Properties
        public int Id { get; set; }  // FK to Site
        public string? ListingItemSelector { get; set; }
        public string? TitleSelector { get; set; }
        public string? PriceSelector { get; set; }
        public string? UrlSelector { get; set; }
        public string? DateSelector { get; set; }
        public string? ListingIdSelector { get; set; }
        public string? CompanySelector { get; set; }
        public string? CitySelector { get; set; }
        public SelectorType SelectorType { get; set; }
        public string Encoding { get; set; }
        
        // Navigation
        public Site? Site { get; set; }
    }
}
```

#### NotificationLog

```csharp
namespace ListingMonitor.Domain.Entities
{
    public class NotificationLog
    {
        // Properties
        public int Id { get; set; }
        public int? RuleId { get; set; }     // Nullable
        public int? ListingId { get; set; }  // Nullable
        public string ToEmail { get; set; }
        public NotificationStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }
        
        // Navigation
        public AlertRule? Rule { get; set; }
        public Listing? Listing { get; set; }
    }
}
```

#### AppSetting

```csharp
namespace ListingMonitor.Domain.Entities
{
    public class AppSetting
    {
        // Properties
        public int Id { get; set; }
        public string Key { get; set; }
        public string? Value { get; set; }
    }
}
```

### 1.2 Enums

```csharp
namespace ListingMonitor.Domain.Enums
{
    public enum SiteType
    {
        AutoSupported = 0,
        Manual = 1
    }
    
    public enum SelectorType
    {
        XPath = 0,
        CssSelector = 1
    }
    
    public enum NotificationStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2
    }
}
```

---

## 2. Application Layer

### 2.1 SiteService

Site CRUD operasyonları ve scraping işlemleri.

```csharp
namespace ListingMonitor.Application.Services
{
    public class SiteService
    {
        // Constructor
        public SiteService(
            AppDbContext context, 
            ISiteScraper modernScraper,
            HttpClient httpClient);
        
        /// <summary>
        /// Tüm siteleri getirir
        /// </summary>
        /// <returns>Site listesi</returns>
        Task<IEnumerable<Site>> GetAllSitesAsync();
        
        /// <summary>
        /// ID'ye göre site getirir
        /// </summary>
        /// <param name="id">Site ID</param>
        /// <returns>Site veya null</returns>
        Task<Site?> GetSiteByIdAsync(int id);
        
        /// <summary>
        /// Yeni site ekler
        /// </summary>
        /// <param name="site">Site entity</param>
        Task AddSiteAsync(Site site);
        
        /// <summary>
        /// Site günceller
        /// </summary>
        /// <param name="site">Güncellenmiş site</param>
        Task UpdateSiteAsync(Site site);
        
        /// <summary>
        /// Site siler (cascade delete listings)
        /// </summary>
        /// <param name="siteId">Silinecek site ID</param>
        Task DeleteSiteAsync(int siteId);
        
        /// <summary>
        /// Belirtilen siteyi scrape eder
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <returns>Bulunan ilan listesi</returns>
        Task<List<ListingDto>> ScrapeSiteAsync(int siteId);
        
        /// <summary>
        /// Sadece aktif siteleri getirir
        /// </summary>
        /// <returns>Aktif site listesi</returns>
        Task<IEnumerable<Site>> GetActiveSitesAsync();
    }
}
```

**Kullanım Örneği:**

```csharp
// Tüm siteleri getir
var sites = await _siteService.GetAllSitesAsync();

// Yeni site ekle
var site = new Site
{
    Name = "Test Site",
    BaseUrl = "https://example.com",
    SiteType = SiteType.Manual,
    IsActive = true,
    CheckIntervalMinutes = 10,
    ParserConfig = new SiteParserConfig
    {
        ListingItemSelector = "//div[@class='item']",
        TitleSelector = ".//h2",
        UrlSelector = ".//a",
        ListingIdSelector = ".//a/@href",
        SelectorType = SelectorType.XPath
    }
};
await _siteService.AddSiteAsync(site);

// Site scrape et
var listings = await _siteService.ScrapeSiteAsync(site.Id);
Console.WriteLine($"Bulunan ilan: {listings.Count}");
```

### 2.2 AlertRuleService

Alarm kuralı CRUD ve eşleştirme operasyonları.

```csharp
namespace ListingMonitor.Application.Services
{
    public class AlertRuleService
    {
        // Constructor
        public AlertRuleService(AppDbContext context);
        
        /// <summary>
        /// Tüm kuralları getirir
        /// </summary>
        Task<IEnumerable<AlertRule>> GetAllRulesAsync();
        
        /// <summary>
        /// ID'ye göre kural getirir
        /// </summary>
        Task<AlertRule?> GetRuleByIdAsync(int id);
        
        /// <summary>
        /// Yeni kural ekler
        /// </summary>
        Task AddRuleAsync(AlertRule rule);
        
        /// <summary>
        /// Kural günceller
        /// </summary>
        Task UpdateRuleAsync(AlertRule rule);
        
        /// <summary>
        /// Kural siler
        /// </summary>
        Task DeleteRuleAsync(int ruleId);
        
        /// <summary>
        /// Aktif kuralları getirir
        /// </summary>
        Task<IEnumerable<AlertRule>> GetActiveRulesAsync();
        
        /// <summary>
        /// İlan kurala uyuyor mu kontrol eder
        /// </summary>
        /// <param name="listing">Kontrol edilecek ilan</param>
        /// <param name="rule">Kural</param>
        /// <returns>Eşleşme durumu</returns>
        bool DoesListingMatchRule(Listing listing, AlertRule rule);
    }
}
```

**Eşleştirme Mantığı:**

```csharp
public bool DoesListingMatchRule(Listing listing, AlertRule rule)
{
    // Site filtresi
    if (rule.SiteId.HasValue && listing.SiteId != rule.SiteId.Value)
        return false;
    
    // Anahtar kelime filtresi (Regex word boundary)
    if (!string.IsNullOrEmpty(rule.Keywords))
    {
        var keywords = rule.Keywords.Split(',').Select(k => k.Trim());
        var titleLower = listing.Title.ToLowerInvariant();
        
        var matched = keywords.Any(keyword =>
        {
            var pattern = $@"\b{Regex.Escape(keyword.ToLowerInvariant())}\b";
            return Regex.IsMatch(titleLower, pattern);
        });
        
        if (!matched) return false;
    }
    
    // Şehir filtresi
    if (!string.IsNullOrEmpty(rule.City) && 
        !listing.City?.Contains(rule.City, StringComparison.OrdinalIgnoreCase) == true)
        return false;
    
    // Fiyat filtresi
    if (rule.MinPrice.HasValue && listing.Price < rule.MinPrice.Value)
        return false;
    if (rule.MaxPrice.HasValue && listing.Price > rule.MaxPrice.Value)
        return false;
    
    return true;
}
```

### 2.3 NotificationService

Email bildirim gönderimi.

```csharp
namespace ListingMonitor.Application.Services
{
    public class NotificationService
    {
        // Constructor
        public NotificationService(
            AppDbContext context, 
            IEmailService emailService);
        
        /// <summary>
        /// Kural ve ilanlar için bildirim gönderir
        /// </summary>
        /// <param name="rule">Alarm kuralı</param>
        /// <param name="matchedListings">Eşleşen ilanlar</param>
        Task SendNotificationAsync(AlertRule rule, IEnumerable<Listing> matchedListings);
        
        /// <summary>
        /// Bildirim loglarını getirir
        /// </summary>
        Task<IEnumerable<NotificationLog>> GetNotificationLogsAsync();
        
        /// <summary>
        /// Son N bildirimi getirir
        /// </summary>
        Task<IEnumerable<NotificationLog>> GetRecentLogsAsync(int count);
    }
}
```

### 2.4 ScraperSchedulerService

Zamanlanmış scraping işlemleri.

```csharp
namespace ListingMonitor.Application.Services
{
    public class ScraperSchedulerService
    {
        // Constructor
        public ScraperSchedulerService(
            AppDbContext context,
            ISiteScraper modernScraper,
            ListingDiffService diffService,
            AlertRuleService alertRuleService,
            NotificationService notificationService,
            HttpClient httpClient);
        
        /// <summary>
        /// Zamanlayıcıyı başlatır
        /// </summary>
        /// <param name="intervalMinutes">Kontrol aralığı (dakika)</param>
        void Start(int intervalMinutes = 10);
        
        /// <summary>
        /// Zamanlayıcıyı durdurur
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Tüm aktif siteleri scrape eder
        /// </summary>
        Task RunNowAsync();
        
        /// <summary>
        /// Tek bir siteyi scrape eder
        /// </summary>
        /// <param name="site">Site entity</param>
        Task ProcessSiteAsync(Site site);
        
        // Events
        event EventHandler<string>? OnStatusChanged;
        event EventHandler<int>? OnProgressChanged;
    }
}
```

**Kullanım Örneği:**

```csharp
// Scheduler başlat
_scheduler.OnStatusChanged += (s, msg) => Console.WriteLine(msg);
_scheduler.Start(intervalMinutes: 10);

// Manuel çalıştır
await _scheduler.RunNowAsync();

// Durdur
_scheduler.Stop();
```

### 2.5 ListingDiffService

Yeni ilan tespiti ve kayıt.

```csharp
namespace ListingMonitor.Application.Services
{
    public class ListingDiffService
    {
        // Constructor
        public ListingDiffService(AppDbContext context);
        
        /// <summary>
        /// Yeni ilanları tespit eder ve kaydeder
        /// </summary>
        /// <param name="site">Site entity</param>
        /// <param name="scrapedListings">Scrape edilen ilanlar</param>
        /// <returns>Yeni ilan listesi</returns>
        Task<List<Listing>> ProcessNewListingsAsync(
            Site site, 
            IEnumerable<ListingDto> scrapedListings);
        
        /// <summary>
        /// İlan daha önce var mı kontrol eder
        /// </summary>
        Task<bool> ExistsAsync(int siteId, string externalId);
    }
}
```

### 2.6 InitialRunEmailService

İlk çalıştırma ve toplu email gönderimi.

```csharp
namespace ListingMonitor.Application.Services
{
    public class InitialRunEmailService
    {
        // Constructor
        public InitialRunEmailService(
            AppDbContext context,
            IEmailService emailService);
        
        /// <summary>
        /// İlk çalıştırma emaili gönderilmiş mi kontrol eder
        /// </summary>
        Task<bool> HasInitialEmailBeenSentAsync();
        
        /// <summary>
        /// İlk çalıştırma emaili gönderir
        /// </summary>
        /// <param name="toEmail">Alıcı email</param>
        Task SendInitialListingsAsync(string toEmail);
        
        /// <summary>
        /// Tüm ilanları veya belirli sitenin ilanlarını gönderir
        /// </summary>
        /// <param name="siteId">Site ID (0 = tüm siteler)</param>
        /// <param name="toEmail">Alıcı email</param>
        Task SendAllListingsAsync(int siteId, string toEmail);
    }
}
```

### 2.7 DatabaseBackupService

Veritabanı yedekleme ve geri yükleme.

```csharp
namespace ListingMonitor.Application.Services
{
    public class DatabaseBackupService
    {
        // Constructor
        public DatabaseBackupService(string connectionString);
        
        /// <summary>
        /// Veritabanı yedeği oluşturur
        /// </summary>
        /// <returns>Yedek dosya yolu</returns>
        Task<string> CreateBackupAsync();
        
        /// <summary>
        /// Sıkıştırılmış yedek oluşturur
        /// </summary>
        /// <returns>Zip dosya yolu</returns>
        Task<string> CreateCompressedBackupAsync();
        
        /// <summary>
        /// Mevcut yedekleri listeler
        /// </summary>
        /// <returns>Yedek dosya listesi</returns>
        IEnumerable<BackupInfo> ListBackups();
        
        /// <summary>
        /// Yedekten geri yükler
        /// </summary>
        /// <param name="backupPath">Yedek dosya yolu</param>
        Task RestoreFromBackupAsync(string backupPath);
        
        /// <summary>
        /// Eski yedekleri temizler
        /// </summary>
        /// <param name="keepCount">Tutulacak yedek sayısı</param>
        Task CleanOldBackupsAsync(int keepCount = 5);
    }
    
    public class BackupInfo
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public DateTime CreatedAt { get; set; }
        public long SizeBytes { get; set; }
    }
}
```

---

## 3. Infrastructure Layer

### 3.1 ISiteScraper Interface

```csharp
namespace ListingMonitor.Infrastructure.Scraping
{
    public interface ISiteScraper
    {
        /// <summary>
        /// Site'den ilanları çeker
        /// </summary>
        /// <param name="site">Site entity</param>
        /// <param name="config">Parser konfigürasyonu (manuel siteler için)</param>
        /// <returns>İlan DTO listesi</returns>
        Task<IEnumerable<ListingDto>> FetchListingsAsync(
            Site site, 
            SiteParserConfig? config = null);
        
        /// <summary>
        /// Adapter'ı test eder
        /// </summary>
        /// <param name="siteName">Site adı</param>
        /// <returns>Erişilebilirlik durumu</returns>
        Task<bool> TestAdapterAsync(string siteName);
    }
}
```

### 3.2 ModernScrapingService

AutoSupported siteler için dispatcher.

```csharp
namespace ListingMonitor.Infrastructure.Scraping
{
    public class ModernScrapingService : ISiteScraper, IDisposable
    {
        // Constructor
        public ModernScrapingService(HttpClient httpClient);
        
        // ISiteScraper implementation
        Task<IEnumerable<ListingDto>> FetchListingsAsync(
            Site site, 
            SiteParserConfig? config = null);
        
        Task<bool> TestAdapterAsync(string siteName);
        
        // IDisposable
        void Dispose();
    }
}
```

**İç Çalışma:**

```csharp
public async Task<IEnumerable<ListingDto>> FetchListingsAsync(
    Site site, SiteParserConfig? config = null)
{
    // Site adına göre adapter seç
    if (site.Name.Contains("Youthall", StringComparison.OrdinalIgnoreCase))
    {
        return await _youthallAdapter.ScrapeListingsAsync();
    }
    else if (site.Name.Contains("lanburda", StringComparison.OrdinalIgnoreCase))
    {
        return await _ilanburdaAdapter.ScrapeListingsAsync();
    }
    else if (site.Name.Contains("Microfon", StringComparison.OrdinalIgnoreCase))
    {
        return await _microfonAdapter.ScrapeListingsAsync();
    }
    
    // Bilinmeyen site
    return Enumerable.Empty<ListingDto>();
}
```

### 3.3 ManualSiteScraper

Manuel siteler için XPath/CSS parser.

```csharp
namespace ListingMonitor.Infrastructure.Scraping
{
    public class ManualSiteScraper : ISiteScraper, IDisposable
    {
        // Constructor
        public ManualSiteScraper(HttpClient httpClient);
        
        /// <summary>
        /// Manuel konfigürasyonla scrape eder
        /// </summary>
        Task<IEnumerable<ListingDto>> FetchListingsAsync(
            Site site, 
            SiteParserConfig? config = null);
        
        /// <summary>
        /// XPath ile element seçer
        /// </summary>
        string? SelectSingleNode(HtmlNode node, string xpath);
        
        /// <summary>
        /// Element'ten text çeker
        /// </summary>
        string? ExtractText(HtmlNode? node, string selector, SelectorType type);
        
        // IDisposable
        void Dispose();
    }
}
```

### 3.4 ISiteAdapter Interface

```csharp
namespace ListingMonitor.Infrastructure.Scraping.Adapters
{
    public interface ISiteAdapter : IDisposable
    {
        /// <summary>
        /// Site adı
        /// </summary>
        string SiteName { get; }
        
        /// <summary>
        /// Site base URL'i
        /// </summary>
        string BaseUrl { get; }
        
        /// <summary>
        /// İlanları scrape eder
        /// </summary>
        Task<IEnumerable<ListingDto>> ScrapeListingsAsync();
        
        /// <summary>
        /// Site erişilebilir mi kontrol eder
        /// </summary>
        Task<bool> IsAvailableAsync();
    }
}
```

### 3.5 YouthallAdapter

```csharp
namespace ListingMonitor.Infrastructure.Scraping.Adapters
{
    public class YouthallAdapter : ISiteAdapter
    {
        // Properties
        public string SiteName => "Youthall";
        public string BaseUrl => "https://youthall.com/tr/talent-programs/";
        
        // Constructor
        public YouthallAdapter(HttpClient httpClient);
        
        // ISiteAdapter implementation
        Task<IEnumerable<ListingDto>> ScrapeListingsAsync();
        Task<bool> IsAvailableAsync();
        void Dispose();
    }
}
```

### 3.6 IlanburdaAdapter

```csharp
namespace ListingMonitor.Infrastructure.Scraping.Adapters
{
    public class IlanburdaAdapter : ISiteAdapter
    {
        // Properties
        public string SiteName => "İlanburda";
        public string BaseUrl => "https://ilanburda.net/8/is-ilanlari";
        
        // Constructor
        public IlanburdaAdapter(HttpClient httpClient);
        
        // ISiteAdapter implementation
        Task<IEnumerable<ListingDto>> ScrapeListingsAsync();
        Task<bool> IsAvailableAsync();
        void Dispose();
    }
}
```

### 3.7 MicrofonAdapter

```csharp
namespace ListingMonitor.Infrastructure.Scraping.Adapters
{
    public class MicrofonAdapter : ISiteAdapter
    {
        // Properties
        public string SiteName => "Microfon";
        public string BaseUrl => "https://microfon.co/scholarship";
        
        // Constructor
        public MicrofonAdapter(HttpClient httpClient);
        
        // ISiteAdapter implementation
        Task<IEnumerable<ListingDto>> ScrapeListingsAsync();
        Task<bool> IsAvailableAsync();
        void Dispose();
        
        // Private methods
        private Task<IEnumerable<ListingDto>> ScrapeFromNextDataAsync(string html);
        private Task<IEnumerable<ListingDto>> ScrapeFromDomAsync(string html);
    }
}
```

### 3.8 IEmailService Interface

```csharp
namespace ListingMonitor.Infrastructure.Email
{
    public interface IEmailService
    {
        /// <summary>
        /// Email gönderir
        /// </summary>
        /// <param name="to">Alıcı</param>
        /// <param name="subject">Konu</param>
        /// <param name="body">HTML içerik</param>
        Task SendEmailAsync(string to, string subject, string body);
        
        /// <summary>
        /// SMTP bağlantısını test eder
        /// </summary>
        /// <returns>Bağlantı durumu</returns>
        Task<bool> TestConnectionAsync();
    }
}
```

### 3.9 SmtpEmailService

```csharp
namespace ListingMonitor.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        // Constructor
        public SmtpEmailService(SmtpSettings settings);
        
        // IEmailService implementation
        Task SendEmailAsync(string to, string subject, string body);
        Task<bool> TestConnectionAsync();
    }
    
    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseStartTls { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
    }
}
```

### 3.10 AppDbContext

```csharp
namespace ListingMonitor.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        // DbSets
        public DbSet<Site> Sites { get; set; }
        public DbSet<SiteParserConfig> SiteParserConfigs { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<AlertRule> AlertRules { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        
        // Constructor
        public AppDbContext(DbContextOptions<AppDbContext> options);
        
        // OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder);
    }
}
```

---

## 4. UI Layer

### 4.1 MainWindowViewModel

```csharp
namespace ListingMonitor.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // Observable Properties
        [ObservableProperty] private bool _isDarkTheme = true;
        [ObservableProperty] private bool _isSchedulerRunning;
        [ObservableProperty] private string _statusMessage;
        [ObservableProperty] private int _selectedTabIndex;
        
        // Collections
        public ObservableCollection<Site> Sites { get; }
        public ObservableCollection<AlertRule> AlertRules { get; }
        public ObservableCollection<Listing> Listings { get; }
        public ObservableCollection<NotificationLog> NotificationLogs { get; }
        public ObservableCollection<string> AvailableSiteFilterOptions { get; }
        public ObservableCollection<Listing> MatchedListings { get; }
        
        // Theme Properties
        public string ThemeBg => IsDarkTheme ? "#0F172A" : "#F1F5F9";
        public string ThemeCardBg => IsDarkTheme ? "#1E293B" : "#FFFFFF";
        public string ThemeText => IsDarkTheme ? "#F1F5F9" : "#1E293B";
        public string ThemeTextSecondary => IsDarkTheme ? "#94A3B8" : "#64748B";
        
        // Commands
        [RelayCommand] private void ToggleScheduler();
        [RelayCommand] private void AddSite();
        [RelayCommand] private void EditSite(Site site);
        [RelayCommand] private Task ScrapeSite(Site site);
        [RelayCommand] private Task ToggleSiteActive(Site site);
        [RelayCommand] private Task DeleteSite(Site site);
        [RelayCommand] private void AddRule();
        [RelayCommand] private void EditRule(AlertRule rule);
        [RelayCommand] private Task DeleteRule(AlertRule rule);
        [RelayCommand] private Task ShowMatchedListings(AlertRule rule);
        [RelayCommand] private Task SendRuleTestEmail(AlertRule rule);
        [RelayCommand] private Task ToggleRuleActive(AlertRule rule);
        [RelayCommand] private void HideMatchedListings();
        [RelayCommand] private Task RefreshData();
        [RelayCommand] private Task SendInitialEmail();
        
        // Methods
        Task LoadDataAsync();
        Task RefreshSitesAsync();
        Task RefreshRulesAsync();
        Task RefreshListingsAsync();
        Task AnalyzeRuleAsync(AlertRule rule);
    }
}
```

### 4.2 SiteEditViewModel

```csharp
namespace ListingMonitor.UI.ViewModels
{
    public partial class SiteEditViewModel : ObservableObject
    {
        // Observable Properties
        [ObservableProperty] private string _siteName;
        [ObservableProperty] private string _baseUrl;
        [ObservableProperty] private int _siteTypeIndex;
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private int _checkIntervalMinutes = 10;
        [ObservableProperty] private bool _isManualSite;
        
        // Parser Config Properties
        [ObservableProperty] private string _listingItemSelector;
        [ObservableProperty] private string _titleSelector;
        [ObservableProperty] private string _urlSelector;
        [ObservableProperty] private string _priceSelector;
        [ObservableProperty] private string _listingIdSelector;
        [ObservableProperty] private string _companySelector;
        [ObservableProperty] private string _citySelector;
        [ObservableProperty] private string _dateSelector;
        [ObservableProperty] private int _selectorTypeIndex;
        
        // Computed Properties
        public string WindowTitle => IsEditing ? "Site Düzenle" : "Yeni Site Ekle";
        public bool IsEditing => _existingSite != null;
        
        // Commands
        [RelayCommand] private Task Save();
        [RelayCommand] private void Cancel();
    }
}
```

### 4.3 AlertRuleEditViewModel

```csharp
namespace ListingMonitor.UI.ViewModels
{
    public partial class AlertRuleEditViewModel : ObservableObject
    {
        // Observable Properties
        [ObservableProperty] private string _ruleName;
        [ObservableProperty] private int _selectedSiteIndex;
        [ObservableProperty] private string _keywords;
        [ObservableProperty] private string _city;
        [ObservableProperty] private decimal? _minPrice;
        [ObservableProperty] private decimal? _maxPrice;
        [ObservableProperty] private string _emailsToNotify;
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private bool _onlyNewListings = true;
        [ObservableProperty] private bool _enableScheduledEmail;
        [ObservableProperty] private int _emailIntervalHours = 6;
        
        // Theme Properties
        [ObservableProperty] private string _themeBg;
        [ObservableProperty] private string _themeCardBg;
        [ObservableProperty] private string _themeText;
        
        // Collections
        public ObservableCollection<SiteFilterOption> SiteOptions { get; }
        
        // Computed Properties
        public string WindowTitle => IsEditing ? "Kuralı Düzenle" : "Yeni Kural Ekle";
        
        // Commands
        [RelayCommand] private Task TestRule();
        [RelayCommand] private Task Save();
        [RelayCommand] private void Cancel();
    }
    
    public class SiteFilterOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
```

---

## 5. DTO'lar

### 5.1 ListingDto

```csharp
namespace ListingMonitor.Application.DTOs
{
    public class ListingDto
    {
        public string ExternalId { get; set; }
        public string Title { get; set; }
        public string? Company { get; set; }
        public decimal? Price { get; set; }
        public string Url { get; set; }
        public string? City { get; set; }
        public DateTime? CreatedAtOnSite { get; set; }
    }
}
```

### 5.2 SiteDto

```csharp
namespace ListingMonitor.Application.DTOs
{
    public class SiteDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string SiteType { get; set; }
        public bool IsActive { get; set; }
        public int ListingCount { get; set; }
    }
}
```

### 5.3 AlertRuleDto

```csharp
namespace ListingMonitor.Application.DTOs
{
    public class AlertRuleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? SiteName { get; set; }
        public string? Keywords { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; }
        public int MatchedCount { get; set; }
    }
}
```

---

## 6. Dependency Injection

### 6.1 Program.cs Konfigürasyonu

```csharp
// Service Collection
var services = new ServiceCollection();

// Database
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// HttpClient
services.AddSingleton<HttpClient>();

// Scraping Services
services.AddSingleton<ISiteScraper, ModernScrapingService>();
services.AddTransient<ManualSiteScraper>();

// Application Services
services.AddScoped<SiteService>();
services.AddScoped<AlertRuleService>();
services.AddScoped<NotificationService>();
services.AddScoped<ListingDiffService>();
services.AddScoped<InitialRunEmailService>();
services.AddScoped<DatabaseBackupService>();
services.AddScoped<ScraperSchedulerService>();

// Email
services.AddSingleton<IEmailService>(sp => 
{
    var settings = LoadSmtpSettings();
    return new SmtpEmailService(settings);
});

// Build Provider
var serviceProvider = services.BuildServiceProvider();
```

### 6.2 Service Lifetime'ları

| Service | Lifetime | Açıklama |
|---------|----------|----------|
| AppDbContext | Scoped | Her request için yeni instance |
| HttpClient | Singleton | Tek instance, connection pooling |
| ModernScrapingService | Singleton | Adapter'ları tutar |
| ManualSiteScraper | Transient | Her çağrıda yeni |
| SiteService | Scoped | DbContext ile aynı scope |
| IEmailService | Singleton | Ayarlar değiştiğinde yeniden oluşturulur |

---

## 7. Event'ler ve Callback'ler

### 7.1 ScraperSchedulerService Events

```csharp
// Status değişikliği
scheduler.OnStatusChanged += (sender, message) =>
{
    Console.WriteLine($"[Status] {message}");
    // UI güncelleme
    Dispatcher.UIThread.Post(() => StatusMessage = message);
};

// İlerleme değişikliği
scheduler.OnProgressChanged += (sender, percentage) =>
{
    Console.WriteLine($"[Progress] {percentage}%");
    // Progress bar güncelleme
    Dispatcher.UIThread.Post(() => Progress = percentage);
};
```

### 7.2 ViewModel Property Changes

```csharp
// INotifyPropertyChanged kullanımı
partial void OnIsDarkThemeChanged(bool value)
{
    // Tema değiştiğinde kaydet
    SaveThemeSettingAsync(value);
    
    // UI'ı güncelle
    OnPropertyChanged(nameof(ThemeBg));
    OnPropertyChanged(nameof(ThemeCardBg));
    OnPropertyChanged(nameof(ThemeText));
}
```

### 7.3 Window Close Callback

```csharp
// SiteEditWindow kapatma callback
var window = new SiteEditWindow(site, async (saved) =>
{
    if (saved)
    {
        await RefreshSitesAsync();
    }
    window.Close();
});
```

---

## 📊 Özet Referans Tablosu

| Katman | Servis/Sınıf | Ana Sorumluluk |
|--------|--------------|----------------|
| Domain | Site | Site entity |
| Domain | Listing | İlan entity |
| Domain | AlertRule | Alarm kuralı entity |
| Application | SiteService | Site CRUD + Scraping |
| Application | AlertRuleService | Kural CRUD + Eşleştirme |
| Application | NotificationService | Email bildirimi |
| Application | ScraperSchedulerService | Zamanlanmış scraping |
| Application | ListingDiffService | Yeni ilan tespiti |
| Infrastructure | ModernScrapingService | AutoSupported scraping |
| Infrastructure | ManualSiteScraper | Manuel site scraping |
| Infrastructure | SmtpEmailService | Email gönderimi |
| Infrastructure | AppDbContext | Veritabanı erişimi |
| UI | MainWindowViewModel | Ana UI mantığı |
| UI | SiteEditViewModel | Site form mantığı |
| UI | AlertRuleEditViewModel | Kural form mantığı |

---

<div align="center">

**API soruları için [GitHub Issues](https://github.com/gokiceynn/NTPProject/issues) kullanın 📚**

</div>
