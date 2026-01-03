using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ListingMonitor.Infrastructure.Scraping;

namespace ListingMonitor.Infrastructure.Scraping.Adapters;

public class MicrofonAdapter : ISiteAdapter
{
    private readonly HttpClient _httpClient;
    private const string BASE = "https://microfon.co";
    
    public string SourceName => "microfon";
    public string BaseUrl => $"{BASE}/scholarship";

    public MicrofonAdapter()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<List<ListingDto>> ScrapeAsync()
    {
        var listings = new List<ListingDto>();
        
        try
        {
            Console.WriteLine("🚀 Microfon __NEXT_DATA__ scraping başlatılıyor...");
            
            var html = await _httpClient.GetStringAsync(BaseUrl);
            
            // __NEXT_DATA__ JSON'ı bul
            var match = Regex.Match(html, @"<script id=""__NEXT_DATA__""[^>]*>([^<]+)</script>");
            
            if (!match.Success)
            {
                Console.WriteLine("⚠️ Microfon: __NEXT_DATA__ bulunamadı, DOM parsing deneniyor...");
                return await ScrapeFromDomAsync(html);
            }
            
            var jsonData = match.Groups[1].Value;
            
            using var doc = JsonDocument.Parse(jsonData);
            var root = doc.RootElement;
            
            // props.pageProps.ssrAds array'ini al
            if (root.TryGetProperty("props", out var props) &&
                props.TryGetProperty("pageProps", out var pageProps) &&
                pageProps.TryGetProperty("ssrAds", out var ssrAds))
            {
                Console.WriteLine($"Microfon: {ssrAds.GetArrayLength()} burs bulundu (JSON)");
                
                var seenIds = new HashSet<string>();
                
                foreach (var ad in ssrAds.EnumerateArray())
                {
                    try
                    {
                        var listing = ExtractListingFromJson(ad);
                        if (listing != null && seenIds.Add(listing.ExternalId))
                        {
                            listings.Add(listing);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Microfon JSON parse hatası: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("⚠️ Microfon: ssrAds bulunamadı");
            }
            
            Console.WriteLine($"🎯 Microfon tamamlandı: Toplam {listings.Count} burs çekildi");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Microfon scraping hatası: {ex.Message}");
        }
        
        return listings;
    }
    
    private ListingDto? ExtractListingFromJson(JsonElement ad)
    {
        var formId = ad.TryGetProperty("formId", out var fid) ? fid.GetString() : null;
        var formIdentityNumber = ad.TryGetProperty("formIdentityNumber", out var fin) ? fin.GetString() : null;
        var title = ad.TryGetProperty("title", out var t) ? t.GetString() : null;
        var company = ad.TryGetProperty("company", out var c) ? c.GetString() : null;
        var explanation = ad.TryGetProperty("explanation", out var e) ? e.GetString() : null;
        var companySlug = ad.TryGetProperty("companySlug", out var cs) ? cs.GetString() : null;
        
        if (string.IsNullOrWhiteSpace(formId) || string.IsNullOrWhiteSpace(title))
            return null;
        
        // Miktar ve para birimi
        var amount = ad.TryGetProperty("amount", out var amt) ? amt.GetString() : null;
        var currencyName = "";
        if (ad.TryGetProperty("currency", out var currency) && 
            currency.TryGetProperty("name", out var cn))
        {
            currencyName = cn.GetString() ?? "";
        }
        
        // Tarih
        var dueDate = "";
        if (ad.TryGetProperty("dueDate", out var dd))
        {
            try
            {
                var date = DateTime.Parse(dd.GetString() ?? "");
                dueDate = date.ToString("dd.MM.yyyy");
            }
            catch { }
        }
        
        // Etiketler
        var tags = new List<string>();
        if (ad.TryGetProperty("tags", out var tagsArr))
        {
            foreach (var tag in tagsArr.EnumerateArray())
            {
                var tagValue = tag.GetString();
                if (!string.IsNullOrWhiteSpace(tagValue))
                    tags.Add(tagValue);
            }
        }
        
        // Detay bilgileri
        var details = new List<string>();
        if (!string.IsNullOrWhiteSpace(amount) && amount != "0,00")
        {
            details.Add($"{amount} {currencyName}".Trim());
        }
        if (!string.IsNullOrWhiteSpace(dueDate))
        {
            details.Add($"Son: {dueDate}");
        }
        if (tags.Any())
        {
            details.Add(string.Join(", ", tags));
        }
        
        // URL oluştur
        var url = $"{BASE}/scholarship/{formIdentityNumber ?? formId}";
        
        return new ListingDto
        {
            Source = SourceName,
            Title = title ?? "",
            Company = company ?? "Microfon Topluluğu",
            Url = url,
            ExternalId = $"microfon_{formId}",
            Description = string.IsNullOrWhiteSpace(explanation) 
                ? string.Join(" | ", details) 
                : explanation,
            ListingType = "scholarship"
        };
    }
    
    // Fallback: DOM'dan parse et
    private Task<List<ListingDto>> ScrapeFromDomAsync(string html)
    {
        return Task.Run(() => ParseDomSync(html));
    }
    
    private List<ListingDto> ParseDomSync(string html)
    {
        var listings = new List<ListingDto>();
        
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        // Güncel DOM: a[href*="/scholarship/"] linkleri
        var links = doc.DocumentNode.SelectNodes("//a[contains(@href,'/scholarship/') and not(contains(@href,'?'))]");
        
        if (links == null)
        {
            Console.WriteLine("⚠️ Microfon: DOM'da link bulunamadı");
            return listings;
        }
        
        var seenUrls = new HashSet<string>();
        
        foreach (var link in links)
        {
            var href = link.GetAttributeValue("href", "");
            if (string.IsNullOrWhiteSpace(href) || !href.StartsWith("/scholarship/"))
                continue;
                
            var fullUrl = BASE + href;
            if (!seenUrls.Add(fullUrl))
                continue;
            
            // Başlık bul
            var titleNode = link.SelectSingleNode(".//h3 | .//h4 | .//span[contains(@class,'title')]");
            var title = titleNode?.InnerText.Trim() ?? link.InnerText.Trim();
            
            if (string.IsNullOrWhiteSpace(title) || title.Length < 3)
                continue;
            
            listings.Add(new ListingDto
            {
                Source = SourceName,
                Title = title,
                Company = "Microfon Topluluğu",
                Url = fullUrl,
                ExternalId = $"microfon_{href.GetHashCode()}",
                Description = "",
                ListingType = "scholarship"
            });
        }
        
        Console.WriteLine($"DOM fallback: {listings.Count} burs bulundu");
        return listings;
    }
    
    private string GenerateExternalId(string url)
    {
        return $"microfon_{url.GetHashCode()}";
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
