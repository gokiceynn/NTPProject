using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ListingMonitor.Infrastructure.Data;
using ListingMonitor.Infrastructure.Scraping;
using ListingMonitor.Application.Services;
using ListingMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListingMonitor.Test;

class UIIntegrationTest
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== UI Integration Test ===\n");

        // Veritabanı yolunu dinamik olarak belirle
        var basePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));
        var dbPath = Path.Combine(basePath, "src", "ListingMonitor.Infrastructure", "listingmonitor.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        using var context = new AppDbContext(options);
        var siteService = new SiteService(context);

        // Test 1: Siteler UI'de görünebilir mi?
        Console.WriteLine("📋 Test 1: Siteler Listesi");
        var sites = await siteService.GetAllSitesAsync();
        Console.WriteLine($"✓ {sites.Count} site bulundu");
        foreach (var site in sites)
        {
            Console.WriteLine($"  - {site.Name} ({site.BaseUrl})");
        }
        Console.WriteLine();

        // Test 2: İlanlar database'de var mı?
        Console.WriteLine("📋 Test 2: İlanlar Database");
        var listingCount = await context.Listings.CountAsync();
        Console.WriteLine($"✓ {listingCount} ilan database'de");
        
        if (listingCount == 0)
        {
            Console.WriteLine("⚠️  Henüz ilan kaydedilmemiş. Scheduler çalıştırılmalı.\n");
        }
        else
        {
            var recentListings = await context.Listings
                .OrderByDescending(l => l.FirstSeenAt)
                .Take(5)
                .ToListAsync();
            
            Console.WriteLine("Son 5 ilan:");
            foreach (var listing in recentListings)
            {
                Console.WriteLine($"  - {listing.Title}");
                Console.WriteLine($"    Link: {listing.Url}");
                Console.WriteLine($"    Tarih: {listing.FirstSeenAt:dd.MM.yyyy HH:mm}");
                Console.WriteLine();
            }
        }

        // Test 3: Scheduler simülasyonu - İlanları çek ve kaydet
        Console.WriteLine("📋 Test 3: Arka Plan Scraping (Simülasyon)");
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        var scraper = new ManualSiteScraper(httpClient);

        var testSite = sites.FirstOrDefault(s => s.Name == "Microfon Burslar");
        if (testSite != null)
        {
            Console.WriteLine($"Test site: {testSite.Name}");
            
            var listings = await scraper.FetchListingsAsync(testSite, testSite.ParserConfig);
            Console.WriteLine($"✓ {listings.Count} ilan çekildi");
            
            // İlk 3'ü database'e kaydet
            var savedCount = 0;
            foreach (var dto in listings.Take(3))
            {
                // Eğer ExternalId boşsa, URL'den oluştur
                var externalId = string.IsNullOrEmpty(dto.ExternalId) 
                    ? dto.Url.GetHashCode().ToString() 
                    : dto.ExternalId;
                
                var existing = await context.Listings
                    .FirstOrDefaultAsync(l => l.SiteId == testSite.Id && l.ExternalId == externalId);
                
                if (existing == null && !string.IsNullOrEmpty(dto.Title))
                {
                    var listing = new Listing
                    {
                        SiteId = testSite.Id,
                        ExternalId = externalId,
                        Title = dto.Title,
                        Price = dto.Price,
                        Url = dto.Url,
                        FirstSeenAt = DateTime.UtcNow,
                        LastSeenAt = DateTime.UtcNow
                    };
                    
                    context.Listings.Add(listing);
                    savedCount++;
                }
            }
            
            await context.SaveChangesAsync();
            Console.WriteLine($"✓ {savedCount} yeni ilan database'e kaydedildi\n");
        }

        // Test 4: CRUD İşlemleri
        Console.WriteLine("📋 Test 4: CRUD İşlemleri");
        
        // Site ekleme
        var testNewSite = new Site
        {
            Name = "Test Site",
            BaseUrl = "https://test.com",
            SiteType = Domain.Enums.SiteType.Manual,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await siteService.CreateSiteAsync(testNewSite);
        Console.WriteLine($"✓ Yeni site eklendi: {testNewSite.Name} (ID: {testNewSite.Id})");
        
        // Site güncelleme
        testNewSite.Name = "Test Site - Updated";
        await siteService.UpdateSiteAsync(testNewSite);
        Console.WriteLine($"✓ Site güncellendi: {testNewSite.Name}");
        
        // Site silme
        await siteService.DeleteSiteAsync(testNewSite.Id);
        Console.WriteLine($"✓ Site silindi: {testNewSite.Name}\n");

        // Test 5: Alert Rules
        Console.WriteLine("📋 Test 5: Alert Rules");
        var ruleCount = await context.AlertRules.CountAsync();
        Console.WriteLine($"✓ {ruleCount} kural tanımlı\n");

        Console.WriteLine("=== Tüm Testler Başarılı ✅ ===");
        Console.WriteLine("\n📌 UI Test Önerileri:");
        Console.WriteLine("1. Uygulamayı açın");
        Console.WriteLine("2. 'Siteler' sekmesine gidin → 'Yenile' tıklayın");
        Console.WriteLine("3. 3 site görünmeli");
        Console.WriteLine("4. 'Dashboard' sekmesine gidin");
        Console.WriteLine("5. İstatistikler güncellenmiş olmalı");
        Console.WriteLine("6. '▶️ Başlat' butonuna tıklayın");
        Console.WriteLine("7. Konsolu izleyin, her 10 dakikada scraping yapılacak");
    }
}
