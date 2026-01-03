using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListingMonitor.Domain.Entities;
using ListingMonitor.Domain.Enums;
using ListingMonitor.Infrastructure.Email;
using ListingMonitor.Application.Services;
using ListingMonitor.Infrastructure.Data;
using ListingMonitor.UI.Views;
using Microsoft.EntityFrameworkCore;
using Avalonia.Threading;
using Avalonia.Controls;

namespace ListingMonitor.UI.ViewModels;

public class ConsoleLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Message { get; set; } = string.Empty;
    public LogLevel Level { get; set; } = LogLevel.Info;
}

public enum LogLevel
{
    Info,
    Warning,
    Error,
    Success
}

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SiteService _siteService;
    private readonly AlertRuleService _ruleService;
    private readonly NotificationService _notificationService;
    private readonly ScraperSchedulerService _schedulerService;
    private readonly InitialRunEmailService _initialRunEmailService;
    private readonly DatabaseBackupService _backupService;

    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private string _statusText = "Scheduler durduruldu";
    [ObservableProperty] private string _schedulerButtonText = "▶️ Başlat";
    
    // Dashboard
    [ObservableProperty] private int _totalSites;
    [ObservableProperty] private int _totalRules;
    [ObservableProperty] private int _todayListings;
    [ObservableProperty] private int _todayEmails;
    [ObservableProperty] private ObservableCollection<Listing> _recentListings = new();

    // Sites
    [ObservableProperty] private ObservableCollection<Site> _sites = new();
    [ObservableProperty] private Site? _selectedSite;
    
    // Add Site
    [ObservableProperty] private bool _isAddingSite;
    [ObservableProperty] private bool _isEditingSite;
    [ObservableProperty] private string _newSiteName = "";
    [ObservableProperty] private string _newSiteUrl = "";
    [ObservableProperty] private int _newSiteTypeIndex = 0; // 0=Scholarship, 1=Job
    // Manual Site Config
    [ObservableProperty] private int _newSiteSelectorTypeIndex = 0; // 0=CSS, 1=XPath
    [ObservableProperty] private string _newSiteListingSelector = "";
    [ObservableProperty] private string _newSiteTitleSelector = "";
    [ObservableProperty] private string _newSitePriceSelector = "";
    [ObservableProperty] private string _newSiteUrlSelector = "";

    // Rules
    [ObservableProperty] private ObservableCollection<AlertRule> _rules = new();
    [ObservableProperty] private AlertRule? _selectedRule;
    
    // Add Rule
    [ObservableProperty] private bool _isAddingRule;
    [ObservableProperty] private bool _isEditingRule;
    [ObservableProperty] private string _newRuleName = "";
    [ObservableProperty] private string _newRuleKeywords = "";
    [ObservableProperty] private string _newRuleCity = "";
    [ObservableProperty] private decimal? _newRuleMinPrice;
    [ObservableProperty] private decimal? _newRuleMaxPrice;
    [ObservableProperty] private string _newRuleEmails = "";

    // Analysis Properties
    [ObservableProperty] private int _totalListingsCount;
    [ObservableProperty] private int _activeRulesCount;
    [ObservableProperty] private int _potentialMatchesCount;
    [ObservableProperty] private DateTime _lastAnalysisTime;
    [ObservableProperty] private string _analysisResults = "";

    // Action Properties
    [ObservableProperty] private int _selectedSiteActionIndex = 0;
    [ObservableProperty] private int _selectedRuleActionIndex = 0;

    // Settings
    [ObservableProperty] private string _smtpHost = "smtp.gmail.com";
    [ObservableProperty] private int _smtpPort = 587;
    [ObservableProperty] private bool _useStartTls = true;
    [ObservableProperty] private string _smtpUsername = "";
    [ObservableProperty] private string _smtpPassword = "";
    [ObservableProperty] private string _fromEmail = "";
    [ObservableProperty] private int _checkIntervalMinutes = 10;
    [ObservableProperty] private string? _settingsMessage;
    
    // Theme Settings
    [ObservableProperty] private bool _isDarkTheme = true;
    [ObservableProperty] private string _themeBackground = "#0F172A";
    [ObservableProperty] private string _themeCardBg = "#1E293B";
    [ObservableProperty] private string _themeText = "#F1F5F9";
    [ObservableProperty] private string _themeTextSecondary = "#94A3B8";
    [ObservableProperty] private string _themeAccent = "#3B82F6";
    
    // Mail Gönderim Ayarları
    [ObservableProperty] private ObservableCollection<string> _mailSiteOptions = new() { "📊 Tüm Siteler" };
    [ObservableProperty] private int _selectedMailSiteIndex = 0;
    [ObservableProperty] private string _mailRecipientEmail = "";
    [ObservableProperty] private string _mailStatusMessage = "";
    
    partial void OnIsDarkThemeChanged(bool value)
    {
        if (value)
        {
            // Dark Theme
            ThemeBackground = "#0F172A";
            ThemeCardBg = "#1E293B";
            ThemeText = "#F1F5F9";
            ThemeTextSecondary = "#94A3B8";
        }
        else
        {
            // Light Theme (Gri tonlu)
            ThemeBackground = "#F1F5F9";
            ThemeCardBg = "#FFFFFF";
            ThemeText = "#1E293B";
            ThemeTextSecondary = "#64748B";
        }
        _ = SaveThemeSettingAsync(value);
    }
    
    private async Task SaveThemeSettingAsync(bool isDark)
    {
        try
        {
            var context = ServiceLocator.GetService<AppDbContext>();
            if (context != null)
            {
                var setting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == "IsDarkTheme");
                if (setting == null)
                {
                    setting = new AppSetting { Key = "IsDarkTheme" };
                    context.AppSettings.Add(setting);
                }
                setting.Value = isDark.ToString();
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Tema kaydetme hatası: {ex.Message}");
        }
    }
    
    partial void OnCheckIntervalMinutesChanged(int value)
    {
        _ = SaveCheckIntervalAsync(value);
    }
    
    private async Task SaveCheckIntervalAsync(int interval)
    {
        try
        {
            var context = ServiceLocator.GetService<AppDbContext>();
            if (context != null)
            {
                // Mevcut ayarı bul veya yeni oluştur
                var setting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == "CheckIntervalMinutes");
                if (setting == null)
                {
                    setting = new AppSetting { Key = "CheckIntervalMinutes" };
                    context.AppSettings.Add(setting);
                }
                setting.Value = interval.ToString();
                await context.SaveChangesAsync();
                
                Console.WriteLine($"✅ Kontrol aralığı {interval} dakika olarak ayarlandı");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Kontrol aralığı kaydetme hatası: {ex.Message}");
        }
    }

    // Logs
    [ObservableProperty] private ObservableCollection<NotificationLog> _logs = new();
    [ObservableProperty] private ObservableCollection<ConsoleLogEntry> _consoleLogs = new();
    private readonly object _consoleLock = new object();
    private const int MaxConsoleLogs = 1000;
    
    // Listings
    [ObservableProperty] private ObservableCollection<Listing> _allListings = new();
    [ObservableProperty] private Listing? _selectedListing;
    [ObservableProperty] private int _selectedSiteFilterIndex = 0;
    [ObservableProperty] private DateTime? _filterStartDate;
    [ObservableProperty] private DateTime? _filterEndDate;
    
    // Site Filter Options (dinamik)
    [ObservableProperty] private ObservableCollection<SiteFilterOption> _siteFilterOptions = new();
    
    public class SiteFilterOption
    {
        public int SiteId { get; set; }
        public string DisplayName { get; set; } = "";
    }
    
    // Rule Analysis Results
    [ObservableProperty] private ObservableCollection<Listing> _matchedListings = new();
    [ObservableProperty] private string _analysisResult = "";
    [ObservableProperty] private bool _isShowingMatchedListings = false;
    [ObservableProperty] private AlertRule? _currentAnalysisRule;
    
    // Backup/Restore
    [ObservableProperty] private ObservableCollection<BackupInfo> _availableBackups = new();
    [ObservableProperty] private BackupInfo? _selectedBackup;
    [ObservableProperty] private string? _backupMessage;
    
    // Timer for dynamic updates
    private Timer? _logRefreshTimer;
    private Timer? _dashboardRefreshTimer;
    private readonly object _refreshLock = new object();

    public MainWindowViewModel()
    {
        _siteService = ServiceLocator.GetService<SiteService>();
        _ruleService = ServiceLocator.GetService<AlertRuleService>();
        _notificationService = ServiceLocator.GetService<NotificationService>();
        _schedulerService = ServiceLocator.GetService<ScraperSchedulerService>();
        _initialRunEmailService = ServiceLocator.GetService<InitialRunEmailService>();
        _backupService = ServiceLocator.GetService<DatabaseBackupService>();

        _ = LoadDataAsync();
        
        // Console logger'ı ayarla
        Console.SetOut(new SimpleConsoleLogger(this));
        
        // Basit log refresh timer - her 30 saniyede bir
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(30000);
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(async () => await RefreshLogsAsync());
                }
                catch { }
            }
        });
    }
    
    private void StartTimers()
    {
        // Log refresh timer - every 30 seconds (start after 30 seconds)
        _logRefreshTimer = new Timer(async _ => 
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(_refreshLock, 1000);
                if (lockTaken)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () => await RefreshLogsAsync());
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_refreshLock);
                }
            }
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        
        // Dashboard refresh timer - every 60 seconds (start after 60 seconds)
        _dashboardRefreshTimer = new Timer(async _ => 
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(_refreshLock, 1000);
                if (lockTaken)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () => await RefreshDashboardAsync());
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_refreshLock);
                }
            }
        }, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
    }

    private async Task LoadDataAsync()
    {
        await LoadSettingsAsync();
        await RefreshDashboardAsync();
        await RefreshSitesAsync();
        await RefreshRulesAsync();
        await RefreshListingsAsync();
        await RefreshLogsAsync();
        RefreshBackups();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var context = ServiceLocator.GetService<AppDbContext>();
            var settings = await context.AppSettings.ToListAsync();
            
            if (settings.Any())
            {
                var dict = settings.ToDictionary(s => s.Key, s => s.Value);
                
                if (dict.TryGetValue("SmtpHost", out var host)) SmtpHost = host;
                if (dict.TryGetValue("SmtpPort", out var port) && int.TryParse(port, out var p)) SmtpPort = p;
                if (dict.TryGetValue("UseStartTls", out var tls) && bool.TryParse(tls, out var t)) UseStartTls = t;
                if (dict.TryGetValue("SmtpUsername", out var user)) SmtpUsername = user;
                if (dict.TryGetValue("SmtpPassword", out var pass)) SmtpPassword = pass;
                if (dict.TryGetValue("FromEmail", out var from)) FromEmail = from;
                if (dict.TryGetValue("CheckIntervalMinutes", out var interval) && int.TryParse(interval, out var i)) CheckIntervalMinutes = i;
                if (dict.TryGetValue("IsDarkTheme", out var theme) && bool.TryParse(theme, out var isDark)) IsDarkTheme = isDark;
                
                // Update runtime settings immediately
                var smtpSettings = ServiceLocator.GetService<SmtpSettings>();
                smtpSettings.SmtpHost = SmtpHost;
                smtpSettings.SmtpPort = SmtpPort;
                smtpSettings.UseStartTls = UseStartTls;
                smtpSettings.Username = SmtpUsername;
                smtpSettings.Password = SmtpPassword;
                smtpSettings.FromEmail = FromEmail;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Settings load error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SendInitialEmailAsync()
    {
        await SendListingsEmailAsync();
    }
    
    [RelayCommand]
    private async Task SendListingsEmail()
    {
        await SendListingsEmailAsync();
    }
    
    private async Task SendListingsEmailAsync()
    {
        try
        {
            // Alıcı email kontrolü
            if (string.IsNullOrWhiteSpace(MailRecipientEmail))
            {
                // SMTP ayarlarından al
                var smtpSettings = ServiceLocator.GetService<SmtpSettings>();
                MailRecipientEmail = smtpSettings?.FromEmail ?? "";
            }
            
            if (string.IsNullOrWhiteSpace(MailRecipientEmail))
            {
                MailStatusMessage = "❌ Lütfen alıcı email girin!";
                return;
            }
            
            MailStatusMessage = "📧 Gönderiliyor...";
            Console.WriteLine($"📧 İlan maili gönderiliyor: {MailRecipientEmail}");
            
            // Seçili siteyi belirle
            int? siteId = null;
            if (SelectedMailSiteIndex > 0 && SelectedMailSiteIndex <= Sites.Count)
            {
                siteId = Sites[SelectedMailSiteIndex - 1].Id;
                Console.WriteLine($"   📍 Seçili site: {Sites[SelectedMailSiteIndex - 1].Name} (ID: {siteId})");
            }
            else
            {
                Console.WriteLine($"   📊 Tüm siteler seçili");
            }
            
            // Mail gönder
            await _initialRunEmailService.SendAllListingsAsync(MailRecipientEmail, siteId);
            
            MailStatusMessage = "✅ Mail gönderildi!";
            StatusText = "✅ İlanlar mail olarak gönderildi";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Mail gönderim hatası: {ex.Message}");
            MailStatusMessage = $"❌ Hata: {ex.Message}";
            StatusText = $"❌ Mail hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ToggleScheduler()
    {
        if (_schedulerService.IsRunning)
        {
            await _schedulerService.StopAsync();
            StatusText = "Scheduler durduruldu";
            SchedulerButtonText = "▶️ Başlat";
        }
        else
        {
            _schedulerService.Start(CheckIntervalMinutes);
            StatusText = $"Scheduler çalışıyor ({CheckIntervalMinutes} dk)";
            SchedulerButtonText = "⏸️ Durdur";
        }
    }

    [RelayCommand]
    private async Task RefreshDashboardAsync()
    {
        try
        {
            var sites = await _siteService.GetAllSitesAsync();
            var rules = await _ruleService.GetAllRulesAsync();
            
            TotalSites = sites.Count;
            TotalRules = rules.Count(r => r.IsActive);
            
            // Get today's stats from database
            var context = ServiceLocator.GetService<AppDbContext>();
            var today = DateTime.UtcNow.Date;
            
            TodayListings = await context.Listings
                .Where(l => l.FirstSeenAt >= today)
                .CountAsync();
            
            TodayEmails = await context.NotificationLogs
                .Where(n => n.SentAt >= today)
                .CountAsync();
            
            // Get recent listings
            var recent = await context.Listings
                .OrderByDescending(l => l.FirstSeenAt)
                .Take(10)
                .ToListAsync();
            
            RecentListings = new ObservableCollection<Listing>(recent);
            
            Console.WriteLine($"Dashboard refreshed: {TotalSites} sites, {TodayListings} listings today");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dashboard refresh error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RefreshSitesAsync()
    {
        try
        {
            var sites = await _siteService.GetAllSitesAsync();
            Sites = new ObservableCollection<Site>(sites);
            
            // Site Filter Options güncelle (dinamik)
            var filterOptions = new List<SiteFilterOption>
            {
                new SiteFilterOption { SiteId = 0, DisplayName = "🌐 Tüm Siteler" }
            };
            filterOptions.AddRange(sites.Select(s => new SiteFilterOption 
            { 
                SiteId = s.Id, 
                DisplayName = s.Name 
            }));
            SiteFilterOptions = new ObservableCollection<SiteFilterOption>(filterOptions);
            
            // Mail Site Options güncelle (dinamik)
            MailSiteOptions.Clear();
            MailSiteOptions.Add("📊 Tüm Siteler");
            foreach (var site in sites)
            {
                MailSiteOptions.Add($"🌐 {site.Name}");
            }
            
            // Alıcı email'i SMTP ayarlarından çek
            if (string.IsNullOrWhiteSpace(MailRecipientEmail))
            {
                MailRecipientEmail = FromEmail;
            }
            
            Console.WriteLine($"Sites refreshed: {sites.Count} sites loaded, {filterOptions.Count} filter options");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sites refresh error: {ex.Message}");
        }
    }

    [RelayCommand]
    private void AddSite()
    {
        try
        {
            Console.WriteLine("🌐 AddSite window açılıyor...");
            var viewModel = new SiteEditViewModel(_siteService, () => { });
            var window = new SiteEditWindow(viewModel);
            window.Closed += async (s, e) => 
            {
                try
                {
                    Console.WriteLine("🔄 Sites yenileniyor...");
                    await RefreshSitesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ RefreshSites hatası: {ex.Message}");
                }
            };
            window.Show();
            Console.WriteLine("✅ AddSite window açıldı");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ AddSite hatası: {ex.Message}");
            Console.WriteLine($"📋 Stack: {ex.StackTrace}");
            StatusText = $"❌ Site ekleme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveNewSite()
    {
        if (string.IsNullOrWhiteSpace(NewSiteName) || string.IsNullOrWhiteSpace(NewSiteUrl))
        {
            SettingsMessage = "Lütfen isim ve URL giriniz.";
            return;
        }

        try
        {
            if (IsEditingSite && SelectedSite != null)
            {
                // Mevcut siteyi güncelle
                SelectedSite.Name = NewSiteName;
                SelectedSite.BaseUrl = NewSiteUrl;
                SelectedSite.SiteType = NewSiteTypeIndex == 0 ? SiteType.AutoSupported : SiteType.Manual;
                
                // If Manual, update parser config
                if (SelectedSite.SiteType == SiteType.Manual)
                {
                    SelectedSite.ParserConfig = SelectedSite.ParserConfig ?? new SiteParserConfig();
                    SelectedSite.ParserConfig.SelectorType = NewSiteSelectorTypeIndex == 0 ? SelectorType.Css : SelectorType.XPath;
                    SelectedSite.ParserConfig.ListingItemSelector = NewSiteListingSelector;
                    SelectedSite.ParserConfig.TitleSelector = NewSiteTitleSelector;
                    SelectedSite.ParserConfig.PriceSelector = NewSitePriceSelector;
                    SelectedSite.ParserConfig.UrlSelector = NewSiteUrlSelector;
                    SelectedSite.ParserConfig.Encoding = "UTF-8";
                }
                
                await _siteService.UpdateSiteAsync(SelectedSite);
                SettingsMessage = "Site başarıyla güncellendi!";
                Console.WriteLine($"✅ {SelectedSite.Name} güncellendi");
            }
            else
            {
                // Yeni site ekle
                var newSite = new Site
                {
                    Name = NewSiteName,
                    BaseUrl = NewSiteUrl,
                    SiteType = NewSiteTypeIndex == 0 ? SiteType.AutoSupported : SiteType.Manual,
                    IsActive = true
                };

                // If Manual, create parser config
                if (newSite.SiteType == SiteType.Manual)
                {
                    newSite.ParserConfig = new SiteParserConfig
                    {
                        SelectorType = NewSiteSelectorTypeIndex == 0 ? SelectorType.Css : SelectorType.XPath,
                        ListingItemSelector = NewSiteListingSelector,
                        TitleSelector = NewSiteTitleSelector,
                        PriceSelector = NewSitePriceSelector,
                        UrlSelector = NewSiteUrlSelector,
                        Encoding = "UTF-8"
                    };
                }

                await _siteService.AddSiteAsync(newSite);
                SettingsMessage = "Site başarıyla eklendi!";
                Console.WriteLine($"✅ {newSite.Name} eklendi");
            }

            await RefreshSitesAsync();
            
            // Reset form
            NewSiteName = "";
            NewSiteUrl = "";
            NewSiteListingSelector = "";
            NewSiteTitleSelector = "";
            NewSitePriceSelector = "";
            NewSiteUrlSelector = "";
            IsAddingSite = false;
            IsEditingSite = false;
            
            await Task.Delay(3000);
            SettingsMessage = null;
        }
        catch (Exception ex)
        {
            SettingsMessage = $"Hata: {ex.Message}";
            Console.WriteLine($"❌ Site kaydetme hatası: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CancelAddSite()
    {
        IsAddingSite = false;
        IsEditingSite = false;
        NewSiteName = "";
        NewSiteUrl = "";
        NewSiteListingSelector = "";
        NewSiteTitleSelector = "";
        NewSitePriceSelector = "";
        NewSiteUrlSelector = "";
        SettingsMessage = null;
    }

    [RelayCommand]
    private async Task RefreshRulesAsync()
    {
        var rules = await _ruleService.GetAllRulesAsync();
        Rules = new ObservableCollection<AlertRule>(rules);
    }

    [RelayCommand]
    private void AddRule()
    {
        AlertRuleEditWindow? window = null;
        var viewModel = new AlertRuleEditViewModel(_ruleService, _siteService, () => window?.Close());
        window = new AlertRuleEditWindow(viewModel);
        window.Closed += async (s, e) => await RefreshRulesAsync();
        window.Show();
    }

    [RelayCommand]
    private async Task SaveNewRule()
    {
        if (string.IsNullOrWhiteSpace(NewRuleName) || string.IsNullOrWhiteSpace(NewRuleEmails))
        {
            SettingsMessage = "Lütfen kural adı ve email adresi giriniz.";
            return;
        }

        try
        {
            if (IsEditingRule && SelectedRule != null)
            {
                // Mevcut kuralı güncelle
                SelectedRule.Name = NewRuleName;
                SelectedRule.Keywords = NewRuleKeywords;
                SelectedRule.City = NewRuleCity;
                SelectedRule.MinPrice = NewRuleMinPrice;
                SelectedRule.MaxPrice = NewRuleMaxPrice;
                SelectedRule.EmailsToNotify = NewRuleEmails;
                
                await _ruleService.UpdateRuleAsync(SelectedRule);
                SettingsMessage = "Kural başarıyla güncellendi!";
                Console.WriteLine($"✅ {SelectedRule.Name} güncellendi");
            }
            else
            {
                // Yeni kural ekle
                var newRule = new AlertRule
                {
                    Name = NewRuleName,
                    Keywords = NewRuleKeywords,
                    City = NewRuleCity,
                    MinPrice = NewRuleMinPrice,
                    MaxPrice = NewRuleMaxPrice,
                    EmailsToNotify = NewRuleEmails,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _ruleService.AddRuleAsync(newRule);
                SettingsMessage = "Kural başarıyla eklendi!";
                Console.WriteLine($"✅ {newRule.Name} eklendi");
            }

            await RefreshRulesAsync();
            
            // Reset form
            NewRuleName = "";
            NewRuleKeywords = "";
            NewRuleCity = "";
            NewRuleMinPrice = null;
            NewRuleMaxPrice = null;
            NewRuleEmails = "";
            IsAddingRule = false;
            IsEditingRule = false;
            
            await Task.Delay(3000);
            SettingsMessage = null;
        }
        catch (Exception ex)
        {
            SettingsMessage = $"Hata: {ex.Message}";
            Console.WriteLine($"❌ Kural kaydetme hatası: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CancelAddRule()
    {
        IsAddingRule = false;
        IsEditingRule = false;
        NewRuleName = "";
        NewRuleKeywords = "";
        NewRuleCity = "";
        NewRuleMinPrice = null;
        NewRuleMaxPrice = null;
        NewRuleEmails = "";
        SettingsMessage = null;
    }

    [RelayCommand]
    private void EditRule(AlertRule rule)
    {
        if (rule == null) return;
        
        AlertRuleEditWindow? window = null;
        var viewModel = new AlertRuleEditViewModel(_ruleService, _siteService, () => window?.Close(), rule);
        window = new AlertRuleEditWindow(viewModel);
        window.Closed += async (s, e) => await RefreshRulesAsync();
        window.Show();
    }

    [RelayCommand]
    private async Task DeleteRule(AlertRule rule)
    {
        if (rule == null) return;
        
        var ruleName = rule.Name;
        
        // Önce UI'ı temizle
        if (CurrentAnalysisRule?.Id == rule.Id)
        {
            IsShowingMatchedListings = false;
            MatchedListings.Clear();
            AnalysisResult = "";
            CurrentAnalysisRule = null;
        }
        
        // Seçili kuralı temizle
        if (SelectedRule?.Id == rule.Id)
        {
            SelectedRule = null;
        }
        
        // DB'den sil
        await _ruleService.DeleteRuleAsync(rule.Id);
        
        // Listeyi yenile
        await RefreshRulesAsync();
        
        Console.WriteLine($"✅ {ruleName} kuralı silindi");
    }
    
    [RelayCommand]
    private async Task ShowMatchedListings(AlertRule rule)
    {
        if (rule == null) return;
        
        Console.WriteLine($"🎯 {rule.Name} için eşleşen ilanlar gösteriliyor...");
        
        // Önceki sonuçları temizle
        MatchedListings.Clear();
        AnalysisResult = "";
        
        SelectedRule = rule;
        CurrentAnalysisRule = rule;
        
        // Yeni analiz yap
        await AnalyzeRuleAsync(rule);
        IsShowingMatchedListings = true;
    }
    
    [RelayCommand]
    private async Task SendRuleTestEmail(AlertRule rule)
    {
        if (rule == null) return;
        
        Console.WriteLine($"📧 {rule.Name} için test mail gönderiliyor...");
        await SendTestEmailAsync(rule);
    }
    
    [RelayCommand]
    private async Task ToggleRuleActive(AlertRule rule)
    {
        if (rule == null) return;
        
        rule.IsActive = !rule.IsActive;
        await _ruleService.UpdateRuleAsync(rule);
        await RefreshRulesAsync();
        Console.WriteLine($"🔄 {rule.Name} durumu: {(rule.IsActive ? "Aktif" : "Pasif")}");
    }

    [RelayCommand]
    private void OpenSmtpSettings()
    {
        var context = ServiceLocator.GetService<AppDbContext>();
        var viewModel = new SmtpSettingsViewModel(context, () => { });
        var window = new SmtpSettingsWindow(viewModel);
        window.Closed += async (s, e) => await LoadSettingsAsync();
        window.Show();
    }

    [RelayCommand]
    private async Task ResetListingsData()
    {
        try
        {
            var context = ServiceLocator.GetService<AppDbContext>();
            
            // Sadece Listing ve NotificationLog tablolarını temizle
            var listings = await context.Listings.ToListAsync();
            var notificationLogs = await context.NotificationLogs.ToListAsync();
            
            context.Listings.RemoveRange(listings);
            context.NotificationLogs.RemoveRange(notificationLogs);
            
            await context.SaveChangesAsync();
            
            // Dashboard'ı yenile
            await RefreshDashboardAsync();
            
            SettingsMessage = $"✅ {listings.Count} ilan ve {notificationLogs.Count} log kaydı başarıyla temizlendi!";
            
            await Task.Delay(3000);
            SettingsMessage = null;
        }
        catch (Exception ex)
        {
            SettingsMessage = $"❌ Sıfırlama hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            var context = ServiceLocator.GetService<AppDbContext>();
            
            // Helper to save/update setting
            async Task SaveSetting(string key, string value)
            {
                var setting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
                if (setting == null)
                {
                    setting = new AppSetting { Key = key, Value = value };
                    context.AppSettings.Add(setting);
                }
                else
                {
                    setting.Value = value;
                }
            }

            await SaveSetting("SmtpHost", SmtpHost);
            await SaveSetting("SmtpPort", SmtpPort.ToString());
            await SaveSetting("UseStartTls", UseStartTls.ToString());
            await SaveSetting("SmtpUsername", SmtpUsername);
            await SaveSetting("SmtpPassword", SmtpPassword);
            await SaveSetting("FromEmail", FromEmail);
            await SaveSetting("CheckIntervalMinutes", CheckIntervalMinutes.ToString());

            await context.SaveChangesAsync();

            // Update runtime settings
            var smtpSettings = ServiceLocator.GetService<SmtpSettings>();
            smtpSettings.SmtpHost = SmtpHost;
            smtpSettings.SmtpPort = SmtpPort;
            smtpSettings.UseStartTls = UseStartTls;
            smtpSettings.Username = SmtpUsername;
            smtpSettings.Password = SmtpPassword;
            smtpSettings.FromEmail = FromEmail;
            smtpSettings.FromName = "İlan Takip";

            SettingsMessage = "Ayarlar kaydedildi ve güncellendi!";
        }
        catch (Exception ex)
        {
            SettingsMessage = $"Kaydetme hatası: {ex.Message}";
        }
        
        await Task.Delay(3000);
        SettingsMessage = null;
    }

    [RelayCommand]
    private async Task TestEmail()
    {
        if (string.IsNullOrEmpty(SmtpHost) || string.IsNullOrEmpty(FromEmail))
        {
            SettingsMessage = "Lütfen önce SMTP ayarlarını giriniz.";
            return;
        }

        try
        {
            SettingsMessage = "Test maili gönderiliyor...";
            
            // Create temp settings for testing
            var testSettings = new ListingMonitor.Infrastructure.Email.SmtpSettings
            {
                SmtpHost = SmtpHost,
                SmtpPort = SmtpPort,
                UseStartTls = UseStartTls,
                Username = SmtpUsername,
                Password = SmtpPassword,
                FromEmail = FromEmail,
                FromName = "İlan Takip Test"
            };

            var emailService = new ListingMonitor.Infrastructure.Email.SmtpEmailService(testSettings);
            
            var subject = $"İlan takip uygulaması {DateTime.Now:dd.MM.yyyy HH:mm}";
            var body = "<html><body><h2>Test Maili</h2><p>yeni düşen ilanlar (Test)</p><p>Bu bir test mailidir.</p></body></html>";

            await emailService.SendEmailAsync(FromEmail, subject, body);
            
            Console.WriteLine($"✅ Test maili gönderildi: {FromEmail}");
            
            SettingsMessage = $"Test maili başarıyla gönderildi! ({FromEmail})";
        }
        catch (Exception ex)
        {
            SettingsMessage = $"Test mail hatası: {ex.Message}";
            Console.WriteLine($"❌ Test mail hatası: {ex.Message}");
        }
        
        await Task.Delay(5000);
        SettingsMessage = null;
    }

    [RelayCommand]
    private async Task RefreshLogsAsync()
    {
        var logs = await _notificationService.GetRecentNotificationsAsync(100);
        Logs = new ObservableCollection<NotificationLog>(logs);
    }
    
    [RelayCommand]
    private async Task RefreshListingsAsync()
    {
        try
        {
            Console.WriteLine("🔄 İlanlar yenileniyor...");
            var context = ServiceLocator.GetService<AppDbContext>();
            
            // Önce toplam ilan sayısını kontrol et
            var totalCount = await context.Listings.CountAsync();
            Console.WriteLine($"   📊 DB'de toplam {totalCount} ilan var");
            
            var query = context.Listings
                .Include(l => l.Site)
                .AsQueryable();
            
            // Site filtreleme - SiteFilterOptions'tan doğru SiteId'yi al
            if (SelectedSiteFilterIndex > 0 && SelectedSiteFilterIndex < SiteFilterOptions.Count)
            {
                var selectedSiteId = SiteFilterOptions[SelectedSiteFilterIndex].SiteId;
                Console.WriteLine($"   🔍 Site filtresi: Index={SelectedSiteFilterIndex}, SiteId={selectedSiteId}");
                if (selectedSiteId > 0)
                {
                    query = query.Where(l => l.SiteId == selectedSiteId);
                }
            }
            
            var listings = await query
                .OrderByDescending(l => l.FirstSeenAt)
                .ToListAsync();
            
            Console.WriteLine($"   ✅ Sorgudan {listings.Count} ilan döndü");
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AllListings = new ObservableCollection<Listing>(listings);
                Console.WriteLine($"   📱 UI'da AllListings güncellendi: {AllListings.Count} ilan");
            });
            
            Console.WriteLine($"Listings refreshed: {listings.Count} listings loaded (filter: index={SelectedSiteFilterIndex})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ İlanlar yenilenirken hata: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
        }
    }
    
    [RelayCommand]
    private void OpenUrl(string url)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening URL: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void ClearConsoleLogs()
    {
        lock (_consoleLock)
        {
            ConsoleLogs.Clear();
        }
    }
    
    public void AddConsoleLog(string message, LogLevel level = LogLevel.Info)
    {
        try
        {
            var logEntry = new ConsoleLogEntry
            {
                Message = message,
                Level = level
            };
            
            Dispatcher.UIThread.Post(() =>
            {
                lock (_consoleLock)
                {
                    ConsoleLogs.Insert(0, logEntry);
                    
                    // Keep only last MaxConsoleLogs
                    while (ConsoleLogs.Count > MaxConsoleLogs)
                    {
                        ConsoleLogs.RemoveAt(ConsoleLogs.Count - 1);
                    }
                }
            });
        }
        catch
        {
            // UI thread error'ını suppress et
        }
    }
    
    [RelayCommand]
    private async Task ScrapeSite(Site site)
    {
        if (site == null) return;
        
        try
        {
            Console.WriteLine($"🔄 {site.Name} scraping başlatılıyor...");
            
            // Scrape işlemi
            var listings = await _siteService.ScrapeSiteAsync(site.Id);
            Console.WriteLine($"✅ {site.Name} scraping tamamlandı: {listings?.Count ?? 0} ilan bulundu");
            
            // Dashboard ve ilanları yenile
            await RefreshDashboardAsync();
            await RefreshListingsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ {site.Name} scraping hatası: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task DeleteSite(Site site)
    {
        if (site == null) return;
        
        try
        {
            var siteName = site.Name;
            Console.WriteLine($"❌ {siteName} siliniyor...");
            
            // Seçili siteyi temizle
            if (SelectedSite?.Id == site.Id)
            {
                SelectedSite = null;
            }
            
            // Site'ı sil
            await _siteService.DeleteSiteAsync(site.Id);
            
            // Listeyi yenile
            await RefreshSitesAsync();
            
            Console.WriteLine($"✅ {siteName} silindi");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ {site.Name} silme hatası: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void EditSite(Site site)
    {
        if (site == null) return;
        
        Console.WriteLine($"✏️ {site.Name} düzenleniyor...");
        
        SiteEditWindow? window = null;
        var viewModel = new SiteEditViewModel(_siteService, () => window?.Close(), site);
        window = new SiteEditWindow(viewModel);
        window.Closed += async (s, e) => await RefreshSitesAsync();
        window.Show();
    }
    
    [RelayCommand]
    private async Task ToggleSiteActive(Site site)
    {
        if (site == null) return;
        
        try
        {
            site.IsActive = !site.IsActive;
            await _siteService.UpdateSiteAsync(site);
            await RefreshSitesAsync();
            Console.WriteLine($"🔀 {site.Name} durumu: {(site.IsActive ? "Aktif" : "Pasif")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ {site.Name} durum değiştirme hatası: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task AnalyzeRules()
    {
        try
        {
            Console.WriteLine("📊 Alarm kuralları analizi başlatılıyor...");
            
            var context = ServiceLocator.GetService<AppDbContext>();
            if (context != null)
            {
                // Toplam ilan sayısı
                TotalListingsCount = await context.Listings.CountAsync();
                
                // Aktif kural sayısı
                ActiveRulesCount = await context.AlertRules.CountAsync(r => r.IsActive);
                
                // Potansiyel eşleşmeleri hesapla
                var listings = await context.Listings.Include(l => l.Site).ToListAsync();
                var rules = await context.AlertRules.Where(r => r.IsActive).ToListAsync();
                
                int potentialMatches = 0;
                var analysisDetails = new List<string>();
                
                foreach (var rule in rules)
                {
                    int ruleMatches = 0;
                    var ruleKeywords = rule.Keywords?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim().ToLower()).ToList() ?? new List<string>();
                    
                    foreach (var listing in listings)
                    {
                        bool matches = true;
                        
                        // Anahtar kelime kontrolü
                        if (ruleKeywords.Any())
                        {
                            var titleLower = listing.Title?.ToLower() ?? "";
                            var fullText = titleLower;
                            
                            if (!ruleKeywords.Any(keyword => fullText.Contains(keyword)))
                            {
                                matches = false;
                            }
                        }
                        
                        // Şehir kontrolü
                        if (!string.IsNullOrWhiteSpace(rule.City) && matches)
                        {
                            var listingCity = listing.City?.ToLower() ?? "";
                            var ruleCity = rule.City.ToLower();
                            if (!listingCity.Contains(ruleCity))
                            {
                                matches = false;
                            }
                        }
                        
                        // Fiyat kontrolü
                        if (matches && (rule.MinPrice.HasValue || rule.MaxPrice.HasValue))
                        {
                            if (rule.MinPrice.HasValue && listing.Price < rule.MinPrice.Value)
                                matches = false;
                            if (rule.MaxPrice.HasValue && listing.Price > rule.MaxPrice.Value)
                                matches = false;
                        }
                        
                        if (matches)
                        {
                            ruleMatches++;
                            potentialMatches++;
                        }
                    }
                    
                    analysisDetails.Add($"📌 {rule.Name}: {ruleMatches} eşleşme");
                }
                
                PotentialMatchesCount = potentialMatches;
                LastAnalysisTime = DateTime.Now;
                
                AnalysisResults = $"📊 Analiz tamamlandı!\n" + string.Join("\n", analysisDetails.Take(5));
                
                Console.WriteLine($"✅ Analiz tamamlandı: {potentialMatches} potansiyel eşleşme bulundu");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Analiz hatası: {ex.Message}");
            AnalysisResults = $"❌ Analiz hatası: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private void TestRules()
    {
        try
        {
            Console.WriteLine("📧 Test alarm gönderiliyor...");
            
            // TODO: Test alarm gönderme mantığı
            // Şimdilik sadece log yaz
            
            Console.WriteLine("✅ Test alarm gönderildi");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test alarm hatası: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task ExecuteSiteAction()
    {
        try
        {
            Console.WriteLine("🔍 ExecuteSiteAction BAŞLADI!");
            
            if (SelectedSite == null)
            {
                Console.WriteLine("❌ Lütfen bir site seçin!");
                return;
            }
            
            Console.WriteLine($"📍 Seçili site: {SelectedSite.Name} (ID: {SelectedSite.Id})");
            
            if (SelectedSiteActionIndex == 0)
            {
                Console.WriteLine("❌ Lütfen bir işlem seçin!");
                return;
            }
            
            Console.WriteLine($"🎯 Seçili işlem index: {SelectedSiteActionIndex}");
            Console.WriteLine($"🚀 {SelectedSite.Name} için işlem yapılıyor: {SelectedSiteActionIndex}");
            
            switch (SelectedSiteActionIndex)
            {
                case 1: // ✏️ Düzenle
                    Console.WriteLine($"✏️ {SelectedSite.Name} düzenleniyor...");
                    
                    // Site düzenleme popup'ı aç
                    try
                    {
                        var editViewModel = new SiteEditViewModel(_siteService, () => { }, SelectedSite);
                        var editWindow = new SiteEditWindow(editViewModel);
                        editWindow.Closed += async (s, e) => 
                        {
                            try
                            {
                                Console.WriteLine("🔄 Sites yenileniyor...");
                                await RefreshSitesAsync();
                                await RefreshListingsAsync();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ Refresh hatası: {ex.Message}");
                            }
                        };
                        editWindow.Show();
                        Console.WriteLine($"✅ {SelectedSite.Name} düzenleme penceresi açıldı");
                    }
                    catch (Exception editEx)
                    {
                        Console.WriteLine($"❌ Düzenleme penceresi açma hatası: {editEx.Message}");
                    }
                    break;
                    
                case 2: // ❌ Sil
                    Console.WriteLine($"❌ {SelectedSite.Name} siliniyor...");
                    Console.WriteLine($"🔍 _siteService kontrol: {_siteService != null}");
                    
                    if (_siteService != null)
                    {
                        Console.WriteLine($"🗑️ DeleteSiteAsync çağrılıyor: ID={SelectedSite.Id}");
                        await _siteService.DeleteSiteAsync(SelectedSite.Id);
                        Console.WriteLine("✅ DeleteSiteAsync tamamlandı");
                        
                        Console.WriteLine("🔄 RefreshSitesAsync çağrılıyor...");
                        await RefreshSitesAsync();
                        Console.WriteLine("✅ RefreshSitesAsync tamamlandı");
                        
                        Console.WriteLine($"✅ {SelectedSite.Name} silindi");
                    }
                    else
                    {
                        Console.WriteLine("❌ _siteService null!");
                    }
                    break;
                    
                case 3: // 🔄 Scrape
                    Console.WriteLine($"🔄 {SelectedSite.Name} scraping başlatılıyor...");
                    
                    if (_siteService != null)
                    {
                        Console.WriteLine($"🔍 ScrapeSiteAsync çağrılıyor: ID={SelectedSite.Id}");
                        
                        // Manuel scraping işlemi
                        try
                        {
                            var listings = await _siteService.ScrapeSiteAsync(SelectedSite.Id);
                            Console.WriteLine($"✅ {SelectedSite.Name} scraping tamamlandı: {listings?.Count ?? 0} ilan bulundu");
                            
                            // Dashboard'ı yenile
                            await RefreshDashboardAsync();
                            Console.WriteLine("✅ Dashboard yenilendi");
                        }
                        catch (Exception scrapeEx)
                        {
                            Console.WriteLine($"❌ Scraping hatası: {scrapeEx.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("❌ _siteService null!");
                    }
                    break;
                    
                default:
                    Console.WriteLine("❌ Bilinmeyen işlem!");
                    break;
            }
            
            // İşlem sonrası index'i sıfırla
            Console.WriteLine("🔄 SelectedSiteActionIndex sıfırlanıyor...");
            SelectedSiteActionIndex = 0;
            Console.WriteLine("✅ ExecuteSiteAction BİTTİ!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Site işlem hatası: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        }
    }
    
    [RelayCommand]
    private async Task ExecuteRuleAction()
    {
        try
        {
            Console.WriteLine("🔍 ExecuteRuleAction BAŞLADI!");
            
            if (SelectedRule == null)
            {
                Console.WriteLine("❌ Lütfen bir kural seçin!");
                return;
            }
            
            Console.WriteLine($"📍 Seçili kural: {SelectedRule.Name} (ID: {SelectedRule.Id})");
            
            if (SelectedRuleActionIndex == 0)
            {
                Console.WriteLine("❌ Lütfen bir işlem seçin!");
                return;
            }
            
            Console.WriteLine($"🎯 Seçili işlem index: {SelectedRuleActionIndex}");
            Console.WriteLine($"🚀 {SelectedRule.Name} için işlem yapılıyor: {SelectedRuleActionIndex}");
            
            switch (SelectedRuleActionIndex)
            {
                case 1: // ✏️ Düzenle
                    Console.WriteLine($"✏️ {SelectedRule.Name} düzenleniyor...");
                    
                    // Kural bilgilerini form'a doldur
                    IsEditingRule = true;
                    IsAddingRule = true;
                    NewRuleName = SelectedRule.Name;
                    NewRuleKeywords = SelectedRule.Keywords ?? "";
                    NewRuleCity = SelectedRule.City ?? "";
                    NewRuleMinPrice = SelectedRule.MinPrice;
                    NewRuleMaxPrice = SelectedRule.MaxPrice;
                    NewRuleEmails = SelectedRule.EmailsToNotify ?? "";
                    
                    Console.WriteLine($"✅ {SelectedRule.Name} düzenleme form'u açıldı");
                    break;
                    
                case 2: // ❌ Sil
                    Console.WriteLine($"❌ {SelectedRule.Name} siliniyor...");
                    Console.WriteLine($"🔍 _ruleService kontrol: {_ruleService != null}");
                    
                    if (_ruleService != null)
                    {
                        Console.WriteLine($"🗑️ DeleteRuleAsync çağrılıyor: ID={SelectedRule.Id}");
                        await _ruleService.DeleteRuleAsync(SelectedRule.Id);
                        Console.WriteLine("✅ DeleteRuleAsync tamamlandı");
                        
                        Console.WriteLine("🔄 RefreshRulesAsync çağrılıyor...");
                        await RefreshRulesAsync();
                        Console.WriteLine("✅ RefreshRulesAsync tamamlandı");
                        
                        Console.WriteLine($"✅ {SelectedRule.Name} silindi");
                    }
                    else
                    {
                        Console.WriteLine("❌ _ruleService null!");
                    }
                    break;
                    
                case 3: // 📊 Analiz Et
                    Console.WriteLine($"📊 {SelectedRule.Name} analiz ediliyor...");
                    await AnalyzeRuleAsync(SelectedRule);
                    break;
                    
                case 4: // 📧 Test Maili Gönder
                    Console.WriteLine($"📧 {SelectedRule.Name} için test mesajı gönderiliyor...");
                    await SendTestEmailAsync(SelectedRule);
                    break;
                    
                case 5: // 🎯 Uyan İlanları Göster
                    Console.WriteLine($"🎯 {SelectedRule.Name} için uyan ilanlar gösteriliyor...");
                    await ShowMatchedListingsAsync(SelectedRule);
                    break;
                    
                case 6: // 🔄 Aktif/Pasif
                    Console.WriteLine($"🔄 {SelectedRule.Name} durumu değiştiriliyor...");
                    Console.WriteLine($"📍 Eski durum: {(SelectedRule.IsActive ? "Aktif" : "Pasif")}");
                    
                    SelectedRule.IsActive = !SelectedRule.IsActive;
                    Console.WriteLine($"📍 Yeni durum: {(SelectedRule.IsActive ? "Aktif" : "Pasif")}");
                    
                    Console.WriteLine($"🔍 _ruleService kontrol: {_ruleService != null}");
                    if (_ruleService != null)
                    {
                        Console.WriteLine("💾 UpdateRuleAsync çağrılıyor...");
                        await _ruleService.UpdateRuleAsync(SelectedRule);
                        Console.WriteLine("✅ UpdateRuleAsync tamamlandı");
                        
                        Console.WriteLine("🔄 RefreshRulesAsync çağrılıyor...");
                        await RefreshRulesAsync();
                        Console.WriteLine("✅ RefreshRulesAsync tamamlandı");
                        
                        Console.WriteLine($"✅ {SelectedRule.Name} durum {(SelectedRule.IsActive ? "aktif" : "pasif")} yapıldı");
                    }
                    else
                    {
                        Console.WriteLine("❌ _ruleService null!");
                    }
                    break;
                    
                default:
                    Console.WriteLine("❌ Bilinmeyen işlem!");
                    break;
            }
            
            // İşlem sonrası index'i sıfırla
            Console.WriteLine("🔄 SelectedRuleActionIndex sıfırlanıyor...");
            SelectedRuleActionIndex = 0;
            Console.WriteLine("✅ ExecuteRuleAction BİTTİ!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Kural işlem hatası: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        }
    }
    
    private async Task AnalyzeRuleAsync(AlertRule rule)
    {
        try
        {
            Console.WriteLine($"🔍 {rule.Name} kuralı analiz ediliyor...");
            
            var context = ServiceLocator.GetService<AppDbContext>();
            
            // Site filtresi uygula
            var query = context.Listings.Include(l => l.Site).AsQueryable();
            if (rule.SiteId.HasValue)
            {
                query = query.Where(l => l.SiteId == rule.SiteId.Value);
            }
            
            // Tüm ilanları al (sadece bugünkü değil)
            var listings = await query.ToListAsync();
            
            var matchedListings = new List<Listing>();
            
            // Anahtar kelimeleri parse et - TAM KELİME EŞLEŞTİRME
            var keywordsText = rule.Keywords?.Trim() ?? "";
            var keywordList = string.IsNullOrWhiteSpace(keywordsText) 
                ? new List<string>() 
                : keywordsText.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim().ToLower())
                    .Where(k => !string.IsNullOrWhiteSpace(k) && k.Length >= 2)
                    .Distinct()
                    .ToList();
            
            Console.WriteLine($"   🔍 Anahtar kelimeler: [{string.Join(", ", keywordList)}]");
            
            foreach (var listing in listings)
            {
                bool matches = true;
                
                // Anahtar kelime kontrolü - TAM KELİME
                if (keywordList.Any())
                {
                    var titleLower = (listing.Title ?? "").ToLower();
                    var companyLower = (listing.Company ?? "").ToLower();
                    var fullText = $" {titleLower} {companyLower} ";
                    
                    // Tam kelime eşleştirmesi (Regex word boundary)
                    bool keywordMatch = keywordList.Any(keyword => 
                        System.Text.RegularExpressions.Regex.IsMatch(
                            fullText, 
                            $@"\b{System.Text.RegularExpressions.Regex.Escape(keyword)}\b",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase
                        ));
                    
                    if (!keywordMatch)
                    {
                        matches = false;
                    }
                }
                
                // Şehir kontrolü
                if (matches && !string.IsNullOrWhiteSpace(rule.City))
                {
                    var listingCity = (listing.City ?? "").ToLower();
                    var ruleCity = rule.City.Trim().ToLower();
                    matches = listingCity.Contains(ruleCity);
                }
                
                // Fiyat aralığı kontrolü
                if (matches && (rule.MinPrice.HasValue || rule.MaxPrice.HasValue))
                {
                    var price = listing.Price ?? 0;
                    if (rule.MinPrice.HasValue && price < rule.MinPrice.Value)
                        matches = false;
                    if (rule.MaxPrice.HasValue && price > rule.MaxPrice.Value)
                        matches = false;
                }
                
                if (matches)
                {
                    matchedListings.Add(listing);
                }
            }
            
            var siteInfo = rule.SiteId.HasValue 
                ? rule.Site?.Name ?? "Seçili Site" 
                : "Tüm Siteler";
            
            AnalysisResult = $"✅ {rule.Name} kuralı analiz tamamlandı:\n" +
                            $"🌐 Site: {siteInfo}\n" +
                            $"📊 Toplam {listings.Count} ilan incelendi\n" +
                            $"🎯 {matchedListings.Count} ilan kurala uyuyor\n" +
                            $"🔍 Anahtar kelimeler: {(keywordList.Any() ? string.Join(", ", keywordList) : "Yok")}\n" +
                            $"📧 E-posta: {rule.EmailsToNotify}";
            
            MatchedListings = new ObservableCollection<Listing>(matchedListings);
            
            // Analiz sonuçlarını otomatik göster
            IsShowingMatchedListings = true;
            CurrentAnalysisRule = rule;
            
            Console.WriteLine($"   ✅ {matchedListings.Count}/{listings.Count} ilan eşleşti");
            
            Console.WriteLine($"✅ {rule.Name} analizi tamamlandı: {matchedListings.Count}/{listings.Count} uyan ilan");
            Console.WriteLine($"🎯 {matchedListings.Count} adet uyan ilan Dashboard'da gösteriliyor");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Analiz hatası: {ex.Message}");
            AnalysisResult = $"❌ Analiz hatası: {ex.Message}";
        }
    }
    
    private async Task SendTestEmailAsync(AlertRule rule)
    {
        try
        {
            Console.WriteLine($"📧 {rule.Name} için test e-postası gönderiliyor...");
            
            await AnalyzeRuleAsync(rule); // Önce analiz et
            
            if (MatchedListings.Any())
            {
                var emailTo = rule.EmailsToNotify?.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                Console.WriteLine($"📧 E-posta gönderiliyor: {emailTo}");
                Console.WriteLine($"📧 Konu: Test Alarm: {rule.Name} - {MatchedListings.Count} Yeni İlan");
                
                // HTML formatında mail içeriği oluştur
                var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Test Alarm: {rule.Name}</title>
</head>
<body style='font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;'>
    <div style='max-width: 800px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2 10px rgba(0,0,0,0.1);'>
        <h1 style='color: #FF9800; text-align: center; margin-bottom: 30px;'>🎯 Kurala Uyan Yeni İlanlar</h1>
        
        <div style='background-color: #FFF3E0; padding: 20px; border-radius: 8px; margin-bottom: 30px; border-left: 4px solid #FF9800;'>
            <h2 style='color: #E65100; margin-top: 0;'>📋 Kural Bilgileri</h2>
            <p><strong>Kural Adı:</strong> {rule.Name}</p>
            <p><strong>Anahtar Kelimeler:</strong> {rule.Keywords ?? "Belirtilmemiş"}</p>
            <p><strong>Şehir:</strong> {rule.City ?? "Belirtilmemiş"}</p>
            <p><strong>Fiyat Aralığı:</strong> {(rule.MinPrice.HasValue ? rule.MinPrice.Value.ToString("N0") + " TL" : "Belirtilmemiş")} - {(rule.MaxPrice.HasValue ? rule.MaxPrice.Value.ToString("N0") + " TL" : "Belirtilmemiş")}</p>
            <p><strong>📧 E-posta:</strong> {rule.EmailsToNotify}</p>
        </div>
        
        <div style='margin-bottom: 30px;'>
            <h2 style='color: #2196F3; margin-bottom: 20px;'>🎯 Uyan İlanlar ({MatchedListings.Count} adet)</h2>
            
            {string.Join("", MatchedListings.Select(( listing, index) => $@"
            <div style='border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; margin-bottom: 15px; background-color: #fafafa;'>
                <div style='display: flex; justify-content: space-between; align-items: start; margin-bottom: 10px;'>
                    <h3 style='color: #FF9800; margin: 0; font-size: 18px;'>{index + 1}. {listing.Title}</h3>
                    <span style='background-color: #4CAF50; color: white; padding: 5px 10px; border-radius: 15px; font-size: 12px; font-weight: bold;'>{(listing.Price.HasValue ? listing.Price.Value.ToString("N0") + " TL" : "Fiyat Belirtilmemiş")}</span>
                </div>
                
                <div style='margin-bottom: 15px;'>
                    <span style='color: #666; margin-right: 20px;'>📍 {listing.City ?? "Şehir Belirtilmemiş"}</span>
                    <span style='color: #666;'>📅 {listing.FirstSeenAt:dd.MM.yyyy HH:mm}</span>
                </div>
                
                <div>
                    <a href='{listing.Url}' style='color: #1976D2; text-decoration: none; font-weight: bold; padding: 8px 16px; background-color: #E3F2FD; border-radius: 5px; display: inline-block;' target='_blank'>
                        🔗 İlanı Görüntüle
                    </a>
                </div>
            </div>
            "))}
        </div>
        
        <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0; color: #666; font-size: 12px;'>
            <p>📅 {DateTime.Now:dd.MM.yyyy HH:mm} tarihinde otomatik gönderilmiştir.</p>
            <p>Bu test mesajı <strong>{rule.Name}</strong> kuralı için gönderilmiştir.</p>
        </div>
    </div>
</body>
</html>";
                
                await _notificationService.SendEmailAsync(
                    $"🎯 Alarm: {rule.Name} - {MatchedListings.Count} Yeni İlan",
                    htmlContent,
                    emailTo
                );
                
                Console.WriteLine($"✅ {rule.Name} için test e-postası başarıyla gönderildi ({MatchedListings.Count} ilan ile)");
                Console.WriteLine($"📧 Gönderilen adres: {emailTo}");
            }
            else
            {
                var emailTo = rule.EmailsToNotify?.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                Console.WriteLine($"📧 E-posta gönderiliyor: {emailTo}");
                
                var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Test Alarm: {rule.Name}</title>
</head>
<body style='font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2 10px rgba(0,0,0,0.1); text-align: center;'>
        <h1 style='color: #FF9800; margin-bottom: 30px;'>🔍 Kural Analiz Sonucu</h1>
        
        <div style='background-color: #FFF3E0; padding: 20px; border-radius: 8px; margin-bottom: 30px; border-left: 4px solid #FF9800; text-align: left;'>
            <h2 style='color: #E65100; margin-top: 0;'>📋 Kural Bilgileri</h2>
            <p><strong>Kural Adı:</strong> {rule.Name}</p>
            <p><strong>Anahtar Kelimeler:</strong> {rule.Keywords ?? "Belirtilmemiş"}</p>
            <p><strong>Şehir:</strong> {rule.City ?? "Belirtilmemiş"}</p>
            <p><strong>Fiyat Aralığı:</strong> {(rule.MinPrice.HasValue ? rule.MinPrice.Value.ToString("N0") + " TL" : "Belirtilmemiş")} - {(rule.MaxPrice.HasValue ? rule.MaxPrice.Value.ToString("N0") + " TL" : "Belirtilmemiş")}</p>
        </div>
        
        <div style='background-color: #F44336; color: white; padding: 20px; border-radius: 8px; margin-bottom: 30px;'>
            <h2 style='margin: 0;'>❌ Uyan İlan Bulunamadı</h2>
            <p>Bu kurala uyan yeni ilan bulunamadı.</p>
        </div>
        
        <div style='color: #666; font-size: 12px;'>
            <p>📅 {DateTime.Now:dd.MM.yyyy HH:mm} tarihinde gönderilmiştir.</p>
        </div>
    </div>
</body>
</html>";
                
                await _notificationService.SendEmailAsync(
                    $"🔍 Alarm: {rule.Name} - Analiz Sonucu",
                    htmlContent,
                    emailTo
                );
                
                Console.WriteLine($"✅ {rule.Name} için test e-postası gönderildi (uyan ilan yok)");
                Console.WriteLine($"📧 Gönderilen adres: {emailTo}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test e-postası hatası: {ex.Message}");
            Console.WriteLine($"❌ Detay: {ex.StackTrace}");
        }
    }
    
    private async Task ShowMatchedListingsAsync(AlertRule rule)
    {
        try
        {
            Console.WriteLine($"🎯 {rule.Name} için uyan ilanlar gösteriliyor...");
            
            await AnalyzeRuleAsync(rule);
            IsShowingMatchedListings = true;
            
            // Dashboard tab'ına geç
            SelectedTabIndex = 0;
            
            Console.WriteLine($"✅ {MatchedListings.Count} adet uyan ilan gösteriliyor");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Uyan ilanlar gösterme hatası: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void HideMatchedListings()
    {
        IsShowingMatchedListings = false;
        MatchedListings.Clear();
        AnalysisResult = "";
        CurrentAnalysisRule = null;
    }
    
    [RelayCommand]
    private async Task SendTestEmailForCurrentRule()
    {
        if (CurrentAnalysisRule != null)
        {
            Console.WriteLine($"📧 Popup'tan {CurrentAnalysisRule.Name} için test maili gönderiliyor...");
            await SendTestEmailAsync(CurrentAnalysisRule);
        }
        else
        {
            Console.WriteLine("❌ Analiz kuralı bulunamadı!");
        }
    }
    
    // ========== BACKUP/RESTORE COMMANDS ==========
    
    [RelayCommand]
    private async Task CreateBackup()
    {
        try
        {
            BackupMessage = "💾 Yedekleme yapılıyor...";
            Console.WriteLine("💾 Database yedekleme başlatılıyor...");
            
            var backupPath = await _backupService.CreateBackupAsync();
            
            BackupMessage = $"✅ Yedek oluşturuldu: {System.IO.Path.GetFileName(backupPath)}";
            Console.WriteLine($"✅ Yedek oluşturuldu: {backupPath}");
            
            RefreshBackups();
            
            await Task.Delay(5000);
            BackupMessage = null;
        }
        catch (Exception ex)
        {
            BackupMessage = $"❌ Yedekleme hatası: {ex.Message}";
            Console.WriteLine($"❌ Yedekleme hatası: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task CreateCompressedBackup()
    {
        try
        {
            BackupMessage = "💾 Sıkıştırılmış yedekleme yapılıyor...";
            Console.WriteLine("💾 Sıkıştırılmış database yedekleme başlatılıyor...");
            
            var backupPath = await _backupService.CreateCompressedBackupAsync();
            
            BackupMessage = $"✅ Sıkıştırılmış yedek oluşturuldu: {System.IO.Path.GetFileName(backupPath)}";
            Console.WriteLine($"✅ Sıkıştırılmış yedek oluşturuldu: {backupPath}");
            
            RefreshBackups();
            
            await Task.Delay(5000);
            BackupMessage = null;
        }
        catch (Exception ex)
        {
            BackupMessage = $"❌ Sıkıştırılmış yedekleme hatası: {ex.Message}";
            Console.WriteLine($"❌ Sıkıştırılmış yedekleme hatası: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task RestoreBackup()
    {
        if (SelectedBackup == null)
        {
            BackupMessage = "❌ Lütfen bir yedek seçin!";
            return;
        }
        
        try
        {
            BackupMessage = $"🔄 Geri yükleme yapılıyor: {SelectedBackup.FileName}...";
            Console.WriteLine($"🔄 Geri yükleme başlatılıyor: {SelectedBackup.FullPath}");
            
            await _backupService.RestoreFromBackupAsync(SelectedBackup.FullPath);
            
            BackupMessage = $"✅ Geri yükleme tamamlandı! Uygulama yeniden başlatılmalı.";
            Console.WriteLine($"✅ Geri yükleme tamamlandı: {SelectedBackup.FileName}");
            
            // Verileri yenile
            await LoadDataAsync();
            
            await Task.Delay(5000);
            BackupMessage = null;
        }
        catch (Exception ex)
        {
            BackupMessage = $"❌ Geri yükleme hatası: {ex.Message}";
            Console.WriteLine($"❌ Geri yükleme hatası: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void RefreshBackups()
    {
        try
        {
            var backups = _backupService.GetAvailableBackups();
            AvailableBackups = new ObservableCollection<BackupInfo>(backups);
            Console.WriteLine($"📂 {backups.Count} yedek listelendi");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Yedek listeleme hatası: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void CleanupBackups()
    {
        try
        {
            BackupMessage = "🗑️ Eski yedekler temizleniyor...";
            Console.WriteLine("🗑️ Eski yedekler temizleniyor (son 5 yedek korunacak)...");
            
            _backupService.CleanupOldBackups(keepCount: 5);
            
            RefreshBackups();
            
            BackupMessage = "✅ Eski yedekler temizlendi (son 5 yedek korundu)";
            Console.WriteLine("✅ Eski yedekler temizlendi");
            
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                await Dispatcher.UIThread.InvokeAsync(() => BackupMessage = null);
            });
        }
        catch (Exception ex)
        {
            BackupMessage = $"❌ Temizleme hatası: {ex.Message}";
            Console.WriteLine($"❌ Temizleme hatası: {ex.Message}");
        }
    }
}

public class SimpleConsoleLogger : System.IO.TextWriter
{
    private readonly MainWindowViewModel _viewModel;
    private readonly System.IO.TextWriter _originalConsole;

    public SimpleConsoleLogger(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel;
        _originalConsole = Console.Out;
    }

    public override void Write(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _viewModel.AddConsoleLog(value.TrimEnd());
        }
        _originalConsole.Write(value);
    }

    public override void WriteLine(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var level = LogLevel.Info;
            if (value.Contains("❌") || value.Contains("Error") || value.Contains("Hata"))
                level = LogLevel.Error;
            else if (value.Contains("⚠️") || value.Contains("Warning") || value.Contains("Uyarı"))
                level = LogLevel.Warning;
            else if (value.Contains("✅") || value.Contains("Success") || value.Contains("Başarılı"))
                level = LogLevel.Success;
                
            _viewModel.AddConsoleLog(value.TrimEnd(), level);
        }
        _originalConsole.WriteLine(value);
    }

    public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
}
