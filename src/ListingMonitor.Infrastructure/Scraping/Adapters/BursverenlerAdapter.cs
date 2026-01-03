using System.Net.Http;
using HtmlAgilityPack;
using ListingMonitor.Infrastructure.Scraping;

namespace ListingMonitor.Infrastructure.Scraping.Adapters;

/// <summary>
/// Bursverenler.org adapter'ı.
/// NOT: Bu site bir burs başvuru platformudur, açık burs listesi sunmamaktadır.
/// Dolayısıyla bu adapter boş liste döner.
/// </summary>
public class BursverenlerAdapter : ISiteAdapter
{
    private readonly HttpClient _httpClient;
    private const string BASE = "https://bursverenler.org";
    
    public string SourceName => "bursverenler";
    public string BaseUrl => $"{BASE}/?lang=tr";

    public BursverenlerAdapter()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<List<ListingDto>> ScrapeAsync()
    {
        // Bursverenler.org bir burs başvuru platformudur.
        // Site açık bir burs listesi sunmamaktadır.
        // Öğrenciler kendi kriterleriyle başvuru yapabilirler ancak
        // scrap edilebilir bir burs listesi bulunmamaktadır.
        
        Console.WriteLine("⚠️ Bursverenler.org: Bu site bir burs başvuru platformudur.");
        Console.WriteLine("   Site açık bir burs listesi sunmamaktadır.");
        Console.WriteLine("   Bu adapter pasif durumda.");
        
        await Task.CompletedTask;
        return new List<ListingDto>();
    }
    
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
