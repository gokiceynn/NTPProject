using System.Net.Http;
using HtmlAgilityPack;
using ListingMonitor.Infrastructure.Scraping;
using System.Linq;
using System.Security.Cryptography;

namespace ListingMonitor.Infrastructure.Scraping.Adapters;

public class YouthallAdapter : ISiteAdapter
{
    private readonly HttpClient _httpClient;
    
    public string SourceName => "youthall";
    public string BaseUrl => "https://www.youthall.com/tr/talent-programs/";

    public YouthallAdapter()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<ListingDto>> ScrapeAsync()
    {
        var listings = new List<ListingDto>();
        
        try
        {
            Console.WriteLine("🚀 Youthall HttpClient scraping başlatılıyor...");
            
            // Önce 1. sayfayı kontrol et ve pagination sayısını bul
            var firstPageHtml = await _httpClient.GetStringAsync(BaseUrl);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(firstPageHtml);
            var totalPages = GetTotalPages(firstPageHtml);
            Console.WriteLine($"📄 Youthall: Toplam {totalPages} sayfa bulundu");
            
            // Tüm sayfaları tara (1-18)
            for (int page = 1; page <= Math.Min(totalPages, 18); page++)
            {
                try
                {
                    var pageUrl = $"{BaseUrl}?page={page}";
                    Console.WriteLine($"📖 Sayfa {page}/{totalPages} işleniyor...");
                    
                    var html = await _httpClient.GetStringAsync(pageUrl);
                    
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    
                    // Gerçek DOM'a göre tüm kartları bul
                    var cards = doc.DocumentNode.SelectNodes("//a[contains(@href, '/tr/') and .//img]");
                    
                    if (cards == null)
                    {
                        Console.WriteLine($"⚠️ Sayfa {page}: Hiç kart bulunamadı");
                        continue;
                    }
                    
                    Console.WriteLine($"   ✅ Sayfa {page}: {cards.Count} ilan bulundu");
                    
                    foreach (var card in cards)
                    {
                        try
                        {
                            var listing = ExtractListingFromCard(card);
                            if (listing != null)
                            {
                                listings.Add(listing);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Kart işleme hatası: {ex.Message}");
                        }
                    }
                    
                    // Rate limiting - siteleri yormamak için
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Sayfa {page} hatası: {ex.Message}");
                }
            }
            
            Console.WriteLine($"🎯 Youthall tamamlandı: Toplam {listings.Count} ilan çekildi");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Youthall scraping hatası: {ex.Message}");
        }
        
        return listings;
    }
    
    private int GetTotalPages(string html)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            // Pagination linklerini bul
            var pageLinks = doc.DocumentNode.SelectNodes("//a[contains(@href, '?page=')]");
            
            if (pageLinks == null)
                return 1;
            
            // Sayfa numaralarını extract et
            var pageNumbers = new List<int>();
            foreach (var link in pageLinks)
            {
                var href = link.GetAttributeValue("href", "");
                var match = System.Text.RegularExpressions.Regex.Match(href, @"page=(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int pageNum))
                {
                    pageNumbers.Add(pageNum);
                }
            }
            
            // En büyük sayfa numarasını dön
            return pageNumbers.Any() ? pageNumbers.Max() : 1;
        }
        catch
        {
            return 1; // Hata durumunda sadece 1 sayfa
        }
    }
    
    private ListingDto? ExtractListingFromCard(HtmlNode card)
    {
        // URL'i al
        var href = card.GetAttributeValue("href", "");
        if (string.IsNullOrWhiteSpace(href) || !href.Contains("/tr/"))
            return null;
            
        // Gerçek DOM yapısına göre text node'ları al
        var textNodes = card.SelectNodes(".//text()[normalize-space()]");
        if (textNodes == null || textNodes.Count < 2)
            return null;
            
        // DOM yapısına göre: [Şirket adı, İlan başlığı, ...]
        var company = textNodes[0]?.InnerText.Trim() ?? string.Empty;
        var title = textNodes[1]?.InnerText.Trim() ?? string.Empty;
        var description = textNodes.Count > 2 ? string.Join(" | ", textNodes.Skip(2).Select(t => t.InnerText.Trim())) : string.Empty;
        
        // Görseller
        var coverImg = card.SelectSingleNode(".//img[1]")?.GetAttributeValue("src", "") ?? string.Empty;
        var logoImg = card.SelectSingleNode(".//img[2]")?.GetAttributeValue("src", "") ?? string.Empty;
        
        var listing = new ListingDto
        {
            Source = SourceName,
            Title = title,
            Company = company,
            Url = href.StartsWith("http") ? href : "https://www.youthall.com" + href,
            ExternalId = GenerateExternalId(href),
            Description = description,
            ListingType = "job"
        };
        
        return listing;
    }
    
    private string GenerateExternalId(string url)
    {
        // URL'deki path kısmını al (domain olmadan)
        var uri = new Uri(url.StartsWith("http") ? url : "https://www.youthall.com" + url);
        var path = uri.AbsolutePath + uri.Query;
        
        // Stable hash için MD5 kullan
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(path));
        var hashString = BitConverter.ToString(hash).Replace("-", "").Substring(0, 8);
        
        return $"youthall_{hashString}";
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
