using ListingMonitor.Domain.Entities;
using ListingMonitor.Domain.Enums;
using ListingMonitor.Infrastructure.Data;
using ListingMonitor.Infrastructure.Scraping;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace ListingMonitor.Application.Services;

public class ScraperSchedulerService
{
    private readonly AppDbContext _context;
    private readonly SiteService _siteService;
    private readonly AlertRuleService _ruleService;
    private readonly NotificationService _notificationService;
    private readonly ISiteScraper _modernScraper;
    private readonly ListingDiffService _diffService;
    private readonly HttpClient _httpClient;
    private PeriodicTimer? _timer;
    private Task? _timerTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public bool IsRunning => _timer != null;

    public ScraperSchedulerService(
        AppDbContext context,
        SiteService siteService,
        AlertRuleService ruleService,
        NotificationService notificationService,
        ISiteScraper scraper,
        ListingDiffService diffService,
        HttpClient httpClient)
    {
        _context = context;
        _siteService = siteService;
        _ruleService = ruleService;
        _notificationService = notificationService;
        _modernScraper = scraper;
        _diffService = diffService;
        _httpClient = httpClient;
    }

    public void Start(int intervalMinutes = 10)
    {
        if (IsRunning)
        {
            Console.WriteLine("Scheduler zaten çalışıyor");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));
        _timerTask = RunSchedulerAsync(_cancellationTokenSource.Token);
        
        Console.WriteLine($"Scheduler başlatıldı ({intervalMinutes} dk interval)");
    }

    public async Task StopAsync()
    {
        if (!IsRunning) return;

        _cancellationTokenSource?.Cancel();
        
        if (_timerTask != null)
        {
            try
            {
                await _timerTask;
            }
            catch (OperationCanceledException)
            {
                // Beklenen durum
            }
        }

        _timer?.Dispose();
        _timer = null;
        _timerTask = null;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        
        Console.WriteLine("Scheduler durduruldu");
    }

    private async Task RunSchedulerAsync(CancellationToken cancellationToken)
    {
        // İlk çalıştırmayı hemen yap
        await ProcessAllSitesAsync();

        while (await _timer!.WaitForNextTickAsync(cancellationToken))
        {
            await ProcessAllSitesAsync();
        }
    }

    private async Task ProcessAllSitesAsync()
    {
        try
        {
            var sites = await _siteService.GetActiveSitesAsync();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {sites.Count} site kontrol ediliyor...");

            foreach (var site in sites)
            {
                await ProcessSiteAsync(site);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Scheduler hatası: {ex.Message}");
        }
    }

    private async Task ProcessSiteAsync(Site site)
    {
        try
        {
            Console.WriteLine($"  → {site.Name} scraping başlatılıyor...");

            // Site tipine göre doğru scraper'ı kullan
            IList<ListingDto> listings;
            
            if (site.SiteType == SiteType.AutoSupported)
            {
                // Modern scraper (Youthall, Microfon, İlanburda)
                Console.WriteLine($"    📡 AutoSupported mod");
                listings = await _modernScraper.FetchListingsAsync(site, site.ParserConfig);
            }
            else
            {
                // Manual scraper (XPath/CSS selector)
                Console.WriteLine($"    📝 Manual mod - XPath scraper");
                if (site.ParserConfig == null)
                {
                    Console.WriteLine($"    ⚠️ ParserConfig bulunamadı, atlanıyor...");
                    return;
                }
                var manualScraper = new ManualSiteScraper(_httpClient);
                listings = await manualScraper.FetchListingsAsync(site, site.ParserConfig);
            }
            
            Console.WriteLine($"    {listings.Count} ilan bulundu");

            // Yeni ilanları tespit et (diff kontrol)
            var newListings = await _diffService.GetNewListingsAsync(site.Id, listings.ToList());
            
            if (newListings.Any())
            {
                // Yeni ilanları kaydet
                await _diffService.SaveNewListingsAsync(site.Id, newListings);
                
                // Alert kurallarını kontrol et ve bildirim gönder
                await ProcessAlertRulesAsync(site, newListings);
            }
            else
            {
                Console.WriteLine($"    📭 {site.Name} için yeni ilan yok");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    ❌ {site.Name} işleme hatası: {ex.Message}");
        }
    }
    
    private async Task ProcessAlertRulesAsync(Site site, List<ListingDto> newListings)
    {
        try
        {
            Console.WriteLine($"    🔔 {newListings.Count} yeni ilan için alert kuralları kontrol ediliyor...");
            
            // Alert kurallarını getir
            var alertRules = await _ruleService.GetActiveRulesBySiteAsync(site.Id);
            
            if (!alertRules.Any())
            {
                Console.WriteLine($"    📋 {site.Name} için aktif alert kuralı yok");
                return;
            }
            
            Console.WriteLine($"    📋 {alertRules.Count} alert kuralı bulundu");
            
            // Her yeni ilan için alert kurallarını kontrol et
            foreach (var listing in newListings)
            {
                foreach (var rule in alertRules)
                {
                    bool matches = false;
                    
                    // Başlık kontrolü
                    if (!string.IsNullOrWhiteSpace(rule.Keywords))
                    {
                        var keywords = rule.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(k => k.Trim().ToLower());
                        matches = keywords.Any(keyword => 
                            listing.Title?.ToLower().Contains(keyword) == true);
                    }
                    
                    if (matches)
                    {
                        Console.WriteLine($"      🎯 Alert: {rule.Name} -> {listing.Title}");
                        
                        // TODO: Email bildirim gönder - Listing entity'si gerekli
                        // await _notificationService.SendListingNotificationAsync(rule, listing);
                        Console.WriteLine($"      📧 Email bildirimi hazırlanıyor: {rule.Name}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Alert işleme hatası: {ex.Message}");
        }
    }
}
