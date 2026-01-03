using ListingMonitor.Infrastructure.Scraping;

namespace ListingMonitor.Infrastructure.Scraping.Adapters;

public interface ISiteAdapter : IDisposable
{
    /// <summary>
    /// Site adı (örn: "youthall", "secretcv", "microfon")
    /// </summary>
    string SourceName { get; }
    
    /// <summary>
    /// Site'nin ana URL'i
    /// </summary>
    string BaseUrl { get; }
    
    /// <summary>
    /// HttpClient ile scraping yapar
    /// </summary>
    Task<List<ListingDto>> ScrapeAsync();
    
    /// <summary>
    /// Site'nin aktif olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IsAvailableAsync();
}
