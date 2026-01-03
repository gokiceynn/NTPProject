using ListingMonitor.Domain.Entities;
using ListingMonitor.Infrastructure.Scraping;
using ListingMonitor.Infrastructure.Scraping.Adapters;

namespace ListingMonitor.Infrastructure.Scraping;

public class ModernScrapingService : ISiteScraper, IDisposable
{
    private readonly YouthallAdapter _youthallAdapter;
    private readonly MicrofonAdapter _microfonAdapter;
    private readonly BursverenlerAdapter _bursverenlerAdapter;
    private readonly IlanburdaAdapter _ilanburdaAdapter;
    
    public ModernScrapingService()
    {
        _youthallAdapter = new YouthallAdapter();
        _microfonAdapter = new MicrofonAdapter();
        _bursverenlerAdapter = new BursverenlerAdapter();
        _ilanburdaAdapter = new IlanburdaAdapter();
    }
    
    public async Task<IList<ListingDto>> FetchListingsAsync(Site site, SiteParserConfig? config)
    {
        var siteName = site.Name.ToLower();
        var siteUrl = site.BaseUrl.ToLower();
        
        // Youthall
        if (siteName.Contains("youthall") || siteUrl.Contains("youthall.com"))
        {
            Console.WriteLine("🚀 Youthall modern adapter kullanılıyor...");
            var listings = await _youthallAdapter.ScrapeAsync();
            Console.WriteLine($"   ✅ {listings.Count} ilan çekildi (Youthall)");
            return listings;
        }
        
        // Microfon
        if (siteName.Contains("microfon") || siteUrl.Contains("microfon.co"))
        {
            Console.WriteLine("🚀 Microfon modern adapter kullanılıyor...");
            var listings = await _microfonAdapter.ScrapeAsync();
            Console.WriteLine($"   ✅ {listings.Count} ilan çekildi (Microfon)");
            return listings;
        }
        
        // Bursverenler
        if (siteName.Contains("bursverenler") || siteUrl.Contains("bursverenler.org"))
        {
            Console.WriteLine("🚀 Bursverenler modern adapter kullanılıyor...");
            var listings = await _bursverenlerAdapter.ScrapeAsync();
            Console.WriteLine($"   ✅ {listings.Count} ilan çekildi (Bursverenler)");
            return listings;
        }
        
        // İlanburda (Türkçe karakter desteği)
        if (siteName.Contains("ilanburda", StringComparison.OrdinalIgnoreCase) || 
            site.Name.Contains("İlanburda", StringComparison.OrdinalIgnoreCase) ||
            siteUrl.Contains("ilanburda.net"))
        {
            Console.WriteLine("🚀 İlanburda modern adapter kullanılıyor...");
            var listings = await _ilanburdaAdapter.ScrapeAsync();
            Console.WriteLine($"   ✅ {listings.Count} ilan çekildi (İlanburda)");
            return listings;
        }
        
        // Diğer siteler için boş liste dön
        Console.WriteLine($"⚠️ {site.Name} için modern adapter mevcut değil");
        return new List<ListingDto>();
    }
    
    /// <summary>
    /// Belirli bir adapter'ın çalışıp çalışmadığını kontrol eder
    /// </summary>
    public async Task<bool> TestAdapterAsync(string adapterName)
    {
        try
        {
            switch (adapterName.ToLower())
            {
                case "youthall":
                    return await _youthallAdapter.IsAvailableAsync();
                case "microfon":
                    return await _microfonAdapter.IsAvailableAsync();
                case "bursverenler":
                    return await _bursverenlerAdapter.IsAvailableAsync();
                case "ilanburda":
                    return await _ilanburdaAdapter.IsAvailableAsync();
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Mevcut adapter listesini döner
    /// </summary>
    public List<string> GetAvailableAdapters()
    {
        return new List<string>
        {
            "youthall",
            "microfon",
            "bursverenler",
            "ilanburda"
        };
    }
    
    public void Dispose()
    {
        _youthallAdapter?.Dispose();
        _microfonAdapter?.Dispose();
        _bursverenlerAdapter?.Dispose();
        _ilanburdaAdapter?.Dispose();
    }
}
