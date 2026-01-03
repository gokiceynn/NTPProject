using System;
using System.Net.Http;
using System.Threading.Tasks;
using ListingMonitor.Domain.Entities;
using ListingMonitor.Domain.Enums;
using ListingMonitor.Infrastructure.Scraping;

namespace QuickInspector;

public class TestEleman
{
    public static async Task TestMethod()
    {
        Console.WriteLine("🔍 Eleman.net Manuel Scraper Test\n");
        
        // HttpClient with User-Agent
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", 
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36");
        
        // Site config
        var site = new Site
        {
            Id = 1,
            Name = "Eleman.net",
            BaseUrl = "https://www.eleman.net/is-ilanlari",
            SiteType = SiteType.Manual,
            IsActive = true
        };
        
        var config = new SiteParserConfig
        {
            SiteId = 1,
            SelectorType = SelectorType.XPath,
            ListingItemSelector = "//div[contains(@class,'ilan_listeleme_bol')]",
            TitleSelector = ".//h3[contains(@class,'c-showcase-box__title')]",
            UrlSelector = ".//a",
            ListingIdSelector = ".//a",
            Encoding = "UTF-8"
        };
        
        Console.WriteLine($"📌 Site: {site.Name}");
        Console.WriteLine($"🔗 URL: {site.BaseUrl}");
        Console.WriteLine($"📝 İlan Kartı: {config.ListingItemSelector}");
        Console.WriteLine($"📝 Başlık: {config.TitleSelector}");
        Console.WriteLine($"📝 URL: {config.UrlSelector}\n");
        
        try
        {
            var scraper = new ManualSiteScraper(httpClient);
            var listings = await scraper.FetchListingsAsync(site, config);
            
            Console.WriteLine($"\n✅ {listings.Count} ilan bulundu!\n");
            
            foreach (var listing in listings.Take(10))
            {
                Console.WriteLine($"  📌 {listing.Title}");
                Console.WriteLine($"     🔗 {listing.Url}");
                Console.WriteLine($"     🆔 {listing.ExternalId}");
                Console.WriteLine();
            }
            
            if (listings.Count > 10)
            {
                Console.WriteLine($"  ... ve {listings.Count - 10} ilan daha");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Hata: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
        }
        
        httpClient.Dispose();
        Console.WriteLine("\n🏁 Test Tamamlandı!");
    }
}
