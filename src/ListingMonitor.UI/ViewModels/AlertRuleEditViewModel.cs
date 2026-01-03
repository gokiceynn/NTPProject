using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListingMonitor.Domain.Entities;
using ListingMonitor.Application.Services;
using ListingMonitor.Infrastructure.Data;
using ListingMonitor.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

namespace ListingMonitor.UI.ViewModels;

public partial class AlertRuleEditViewModel : ObservableObject
{
    private readonly AlertRuleService _ruleService;
    private readonly SiteService _siteService;
    private readonly Action _onClose;
    
    [ObservableProperty] private AlertRule? _rule;
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _keywords = "";
    [ObservableProperty] private decimal? _minPrice;
    [ObservableProperty] private decimal? _maxPrice;
    [ObservableProperty] private string _city = "";
    [ObservableProperty] private string _emailsToNotify = "";
    [ObservableProperty] private bool _isActive = true;
    [ObservableProperty] private bool _onlyNewListings = true;
    [ObservableProperty] private int? _siteId = null; // null = tüm siteler
    
    // Mail zamanlaması
    [ObservableProperty] private bool _enableScheduledEmail = false;
    [ObservableProperty] private int _emailIntervalHours = 6; // default 6 saat
    
    public int EmailIntervalHoursIndex
    {
        get => EmailIntervalHours switch
        {
            1 => 0,
            6 => 1,
            12 => 2,
            24 => 3,
            _ => 1
        };
        set => EmailIntervalHours = value switch
        {
            0 => 1,
            1 => 6,
            2 => 12,
            3 => 24,
            _ => 6
        };
    }
    
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private List<Site> _availableSites = new();
    [ObservableProperty] private int _selectedSiteIndex = 0; // 0 = Tüm Siteler
    [ObservableProperty] private string _windowTitle = "📋 Yeni Kural Ekle";
    [ObservableProperty] private int _matchedListingsCount = 0;
    [ObservableProperty] private string _testResultMessage = "";
    [ObservableProperty] private List<string> _siteOptions = new() { "🌐 Tüm Siteler" };
    
    // Tema ayarları
    [ObservableProperty] private bool _isDarkTheme = true;
    [ObservableProperty] private string _themeBg = "#0F172A";
    [ObservableProperty] private string _themeCardBg = "#1E293B";
    [ObservableProperty] private string _themeText = "#F1F5F9";
    [ObservableProperty] private string _themeTextSecondary = "#94A3B8";
    [ObservableProperty] private string _themeInputBg = "#0F172A";
    [ObservableProperty] private string _themeBorder = "#334155";

    public AlertRuleEditViewModel(AlertRuleService ruleService, SiteService siteService, Action onClose, AlertRule? rule = null)
    {
        _ruleService = ruleService;
        _siteService = siteService;
        _onClose = onClose;
        _rule = rule;
        _isEditing = rule != null;
        
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Tema ayarlarını yükle
            var context = ServiceLocator.GetService<AppDbContext>();
            var themeSetting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == "IsDarkTheme");
            IsDarkTheme = themeSetting?.Value != "False";
            ApplyTheme();
            
            // Load available sites
            var sites = await _siteService.GetAllSitesAsync();
            AvailableSites = sites.ToList();
            IsEditing = Rule != null;
            
            // Window başlığını ayarla
            WindowTitle = IsEditing ? "✏️ Kuralı Düzenle" : "📋 Yeni Kural Ekle";
            
            // Site options güncelle (dinamik)
            var options = new List<string> { "🌐 Tüm Siteler" };
            options.AddRange(AvailableSites.Select(s => $"📍 {s.Name}"));
            SiteOptions = options;
        
            if (IsEditing)
            {
                LoadRuleData();
            }
        }
        catch (Exception ex)
        {
            Message = $"Veri yükleme hatası: {ex.Message}";
        }
    }
    
    private void ApplyTheme()
    {
        if (IsDarkTheme)
        {
            ThemeBg = "#0F172A";
            ThemeCardBg = "#1E293B";
            ThemeText = "#F1F5F9";
            ThemeTextSecondary = "#94A3B8";
            ThemeInputBg = "#0F172A";
            ThemeBorder = "#334155";
        }
        else
        {
            ThemeBg = "#F1F5F9";
            ThemeCardBg = "#FFFFFF";
            ThemeText = "#1E293B";
            ThemeTextSecondary = "#64748B";
            ThemeInputBg = "#FFFFFF";
            ThemeBorder = "#E2E8F0";
        }
    }

    private void LoadRuleData()
    {
        if (Rule == null) return;
        
        Name = Rule.Name;
        Keywords = Rule.Keywords ?? "";
        MinPrice = Rule.MinPrice;
        MaxPrice = Rule.MaxPrice;
        City = Rule.City ?? "";
        EmailsToNotify = Rule.EmailsToNotify;
        IsActive = Rule.IsActive;
        OnlyNewListings = Rule.OnlyNewListings;
        SiteId = Rule.SiteId;
        
        // Mail zamanlaması ayarları
        EnableScheduledEmail = Rule.EnableScheduledEmail;
        EmailIntervalHours = Rule.EmailIntervalHours ?? 6;
        
        // Set selected site index
        if (SiteId.HasValue)
        {
            var siteIndex = AvailableSites.FindIndex(s => s.Id == SiteId.Value);
            SelectedSiteIndex = siteIndex >= 0 ? siteIndex + 1 : 0; // +1 because 0 is "Tüm Siteler"
        }
        else
        {
            SelectedSiteIndex = 0; // Tüm Siteler
        }
    }

    partial void OnSelectedSiteIndexChanged(int value)
    {
        if (value == 0)
        {
            SiteId = null; // Tüm Siteler
        }
        else if (value > 0 && value <= AvailableSites.Count)
        {
            SiteId = AvailableSites[value - 1].Id;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(EmailsToNotify))
            {
                Message = "Lütfen kural adı ve bildirim email adresi giriniz.";
                return;
            }

            // Validate email format (basic check)
            var emails = EmailsToNotify.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => e.Contains('@'))
                .ToList();

            if (!emails.Any())
            {
                Message = "Lütfen geçerli email adresleri giriniz (virgülle ayırın).";
                return;
            }

            if (IsEditing && Rule != null)
            {
                // Update existing rule
                Rule.Name = Name;
                Rule.Keywords = string.IsNullOrWhiteSpace(Keywords) ? null : Keywords;
                Rule.MinPrice = MinPrice;
                Rule.MaxPrice = MaxPrice;
                Rule.City = string.IsNullOrWhiteSpace(City) ? null : City;
                Rule.EmailsToNotify = string.Join(", ", emails);
                Rule.IsActive = IsActive;
                Rule.OnlyNewListings = OnlyNewListings;
                Rule.SiteId = SiteId;
                
                // Mail zamanlaması ayarları
                Rule.EnableScheduledEmail = EnableScheduledEmail;
                Rule.EmailIntervalHours = EnableScheduledEmail ? EmailIntervalHours : null;
                Rule.NextEmailSendAt = EnableScheduledEmail ? DateTime.UtcNow.AddHours(EmailIntervalHours) : null;

                await _ruleService.UpdateRuleAsync(Rule);
                Message = "Kural başarıyla güncellendi!";
            }
            else
            {
                // Create new rule
                var newRule = new AlertRule
                {
                    Name = Name,
                    Keywords = string.IsNullOrWhiteSpace(Keywords) ? null : Keywords,
                    MinPrice = MinPrice,
                    MaxPrice = MaxPrice,
                    City = string.IsNullOrWhiteSpace(City) ? null : City,
                    EmailsToNotify = string.Join(", ", emails),
                    IsActive = IsActive,
                    OnlyNewListings = OnlyNewListings,
                    SiteId = SiteId,
                    CreatedAt = DateTime.UtcNow,
                    
                    // Mail zamanlaması ayarları
                    EnableScheduledEmail = EnableScheduledEmail,
                    EmailIntervalHours = EnableScheduledEmail ? EmailIntervalHours : null,
                    NextEmailSendAt = EnableScheduledEmail ? DateTime.UtcNow.AddHours(EmailIntervalHours) : null
                };

                await _ruleService.AddRuleAsync(newRule);
                Message = "Kural başarıyla eklendi!";
            }

            await Task.Delay(2000);
            _onClose?.Invoke();
        }
        catch (Exception ex)
        {
            Message = $"Hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onClose?.Invoke();
    }

    [RelayCommand]
    private async Task TestRule()
    {
        try
        {
            TestResultMessage = "🔍 Test ediliyor...";
            
            // Debug
            Console.WriteLine($"🧪 Kural Test Başlıyor:");
            Console.WriteLine($"   📝 Keywords raw: '{Keywords}'");
            Console.WriteLine($"   📍 City: '{City}'");
            Console.WriteLine($"   🌐 SiteId: {SiteId}");
            
            var context = ServiceLocator.GetService<AppDbContext>();
            var query = context.Listings.Include(l => l.Site).AsQueryable();
            
            // Site filtresi
            if (SiteId.HasValue)
            {
                query = query.Where(l => l.SiteId == SiteId.Value);
            }
            
            var allListings = await query.ToListAsync();
            var matchedListings = new List<Listing>();
            
            // Anahtar kelime filtresi - trim ve parse
            var keywordsText = Keywords?.Trim() ?? "";
            var keywordList = string.IsNullOrWhiteSpace(keywordsText) 
                ? new List<string>() 
                : keywordsText.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim().ToLower())
                    .Where(k => !string.IsNullOrWhiteSpace(k) && k.Length >= 2)
                    .Distinct()
                    .ToList();
            
            Console.WriteLine($"   🔍 Parsed keywords ({keywordList.Count}): [{string.Join(", ", keywordList)}]");
            
            // Hiç filtre yoksa uyarı ver
            bool hasAnyFilter = keywordList.Any() || !string.IsNullOrWhiteSpace(City) || MinPrice.HasValue || MaxPrice.HasValue;
            
            if (!hasAnyFilter)
            {
                TestResultMessage = "⚠️ Hiçbir filtre kriteri girilmedi!\n\nLütfen en az bir kriter girin:\n• Anahtar kelimeler\n• Şehir\n• Min/Max fiyat";
                Message = "⚠️ Filtre kriteri gerekli";
                MatchedListingsCount = 0;
                return;
            }
            
            foreach (var listing in allListings)
            {
                bool matches = true;
                
                // Anahtar kelime kontrolü - TAM KELİME EŞLEŞTİRME
                if (keywordList.Any())
                {
                    var titleLower = (listing.Title ?? "").ToLower();
                    var companyLower = (listing.Company ?? "").ToLower();
                    var fullText = $" {titleLower} {companyLower} "; // Başa ve sona boşluk ekle
                    
                    // Herhangi bir anahtar kelime TAM OLARAK içermeli
                    bool keywordMatch = keywordList.Any(keyword => 
                    {
                        // Tam kelime eşleştirmesi için regex benzeri kontrol
                        // "burs" -> " burs " veya " burs," veya " bursu " vb.
                        var patterns = new[] {
                            $" {keyword} ",      // tam kelime
                            $" {keyword},",      // kelime sonra virgül
                            $" {keyword}.",      // kelime sonra nokta
                            $" {keyword})",      // kelime sonra parantez
                            $" {keyword}-",      // kelime sonra tire
                            $"({keyword}",       // parantez içinde başlayan
                            $" {keyword}u ",     // Türkçe eklentiler (bursu)
                            $" {keyword}ı ",     // (bursı - yanlış ama olabilir)
                            $" {keyword}ları ",  // çoğul (bursları)
                            $" {keyword}leri ",  // çoğul (bursleri)
                        };
                        
                        return patterns.Any(p => fullText.Contains(p)) ||
                               // Ayrıca: kelime tam olarak title'ın bir parçasıysa
                               System.Text.RegularExpressions.Regex.IsMatch(
                                   fullText, 
                                   $@"\b{System.Text.RegularExpressions.Regex.Escape(keyword)}\b",
                                   System.Text.RegularExpressions.RegexOptions.IgnoreCase
                               );
                    });
                    
                    if (!keywordMatch)
                    {
                        matches = false;
                    }
                }
                
                // Şehir kontrolü
                if (!string.IsNullOrWhiteSpace(City) && matches)
                {
                    var listingCity = (listing.City ?? "").ToLower();
                    var ruleCity = City.Trim().ToLower();
                    if (!listingCity.Contains(ruleCity))
                    {
                        matches = false;
                    }
                }
                
                // Fiyat kontrolü
                if (matches && (MinPrice.HasValue || MaxPrice.HasValue))
                {
                    var price = listing.Price ?? 0;
                    if (MinPrice.HasValue && price < MinPrice.Value)
                        matches = false;
                    if (MaxPrice.HasValue && price > MaxPrice.Value)
                        matches = false;
                }
                
                if (matches)
                {
                    matchedListings.Add(listing);
                }
            }
            
            MatchedListingsCount = matchedListings.Count;
            Console.WriteLine($"   ✅ Eşleşen: {matchedListings.Count}/{allListings.Count}");
            
            // Sonuç mesajı
            var siteInfo = SiteId.HasValue 
                ? AvailableSites.FirstOrDefault(s => s.Id == SiteId.Value)?.Name ?? "Seçili Site" 
                : "Tüm Siteler";
            
            TestResultMessage = $"✅ Test Sonucu:\n" +
                               $"📊 Toplam İlan: {allListings.Count}\n" +
                               $"🎯 Eşleşen: {matchedListings.Count}\n" +
                               $"🌐 Site: {siteInfo}";
            
            if (keywordList.Any())
            {
                TestResultMessage += $"\n🔍 Anahtar Kelimeler: {string.Join(", ", keywordList)}";
            }
            
            if (!string.IsNullOrWhiteSpace(City))
            {
                TestResultMessage += $"\n📍 Şehir: {City}";
            }
            
            if (matchedListings.Any())
            {
                TestResultMessage += $"\n\n📋 Örnek Eşleşmeler (ilk 5):";
                foreach (var listing in matchedListings.Take(5))
                {
                    var title = listing.Title ?? "";
                    var displayTitle = title.Length > 50 ? title.Substring(0, 47) + "..." : title;
                    TestResultMessage += $"\n  • {displayTitle}";
                }
            }
            else if (keywordList.Any())
            {
                TestResultMessage += $"\n\n⚠️ '{string.Join(", ", keywordList)}' kelimelerini içeren ilan bulunamadı.";
            }
            
            Message = matchedListings.Count > 0 
                ? $"✅ {matchedListings.Count} ilan bu kurala uyuyor" 
                : "⚠️ Kriterlere uyan ilan bulunamadı";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test hatası: {ex.Message}");
            TestResultMessage = $"❌ Test hatası: {ex.Message}";
            Message = $"❌ Test hatası: {ex.Message}";
        }
    }
}
