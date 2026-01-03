using System.Net.Http;
using System.Web;
using HtmlAgilityPack;
using ListingMonitor.Infrastructure.Scraping;

namespace ListingMonitor.Infrastructure.Scraping.Adapters;

public class IlanburdaAdapter : ISiteAdapter
{
    private readonly HttpClient _httpClient;
    private const string BASE = "https://www.ilanburda.net";
    
    public string SourceName => "ilanburda";
    public string BaseUrl => $"{BASE}/8/is-ilanlari";

    public IlanburdaAdapter()
    {
        _httpClient = new HttpClient();
        // İlanburda User-Agent kontrolü yapıyor - gerçekçi bir User-Agent gerekli
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
    }

    public async Task<List<ListingDto>> ScrapeAsync()
    {
        var listings = new List<ListingDto>();
        
        try
        {
            Console.WriteLine($"🔍 İlanburda scraping başlıyor: {BaseUrl}");
            
            var html = await _httpClient.GetStringAsync(BaseUrl);
            Console.WriteLine($"📄 HTML alındı: {html.Length} karakter");
            
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // İlanlar <tr class="satir_link"> içinde
            var ilanlar = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'satir_link')]");
            
            if (ilanlar == null || ilanlar.Count == 0)
            {
                Console.WriteLine("⚠️ Hiç ilan bulunamadı - XPath: //tr[contains(@class, 'satir_link')]");
                return listings;
            }

            Console.WriteLine($"✅ {ilanlar.Count} ilan bulundu");

            foreach (var item in ilanlar)
            {
                try
                {
                    // data-href attribute'undan URL al
                    var url = item.GetAttributeValue("data-href", "");
                    if (string.IsNullOrEmpty(url))
                    {
                        continue;
                    }

                    // Başlık: <span class="baslik">
                    var baslikNode = item.SelectSingleNode(".//span[contains(@class, 'baslik')]");
                    var title = baslikNode?.InnerText?.Trim() ?? "";
                    
                    if (string.IsNullOrEmpty(title))
                    {
                        continue;
                    }
                    
                    // HTML decode
                    title = HttpUtility.HtmlDecode(title);

                    // Fiyat: <td class="fiyat">
                    var fiyatNode = item.SelectSingleNode(".//td[contains(@class, 'fiyat')]");
                    var priceText = fiyatNode?.InnerText?.Trim() ?? "";
                    
                    // Şehir/İlçe: <div class="il_ilce"> içindeki <span>'lar
                    var ilIlceNode = item.SelectSingleNode(".//div[contains(@class, 'il_ilce')]");
                    var il = "";
                    var ilce = "";
                    if (ilIlceNode != null)
                    {
                        var spans = ilIlceNode.SelectNodes(".//span");
                        if (spans != null && spans.Count >= 1)
                        {
                            il = spans[0]?.InnerText?.Trim() ?? "";
                        }
                        if (spans != null && spans.Count >= 2)
                        {
                            ilce = spans[1]?.InnerText?.Trim() ?? "";
                        }
                    }
                    var city = !string.IsNullOrEmpty(ilce) ? $"{il}/{ilce}" : il;

                    // URL'den ID çıkar (örn: rehber-ogretmeni-1650 -> 1650)
                    var id = ExtractIdFromUrl(url);
                    if (string.IsNullOrEmpty(id))
                    {
                        id = url.GetHashCode().ToString();
                    }

                    // Fiyatı parse et
                    decimal? price = null;
                    if (!string.IsNullOrEmpty(priceText))
                    {
                        priceText = priceText.Replace("TL", "").Replace("₺", "").Replace(".", "").Replace(",", ".").Trim();
                        if (decimal.TryParse(priceText, out var parsedPrice))
                        {
                            price = parsedPrice;
                        }
                    }

                    listings.Add(new ListingDto
                    {
                        Source = SourceName,
                        Title = title,
                        Price = price,
                        Url = url.StartsWith("http") ? url : $"{BASE}{url}",
                        ExternalId = $"ilanburda_{id}",
                        City = city,
                        Company = "",
                        ListingType = "job"
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ İlan parse hatası: {ex.Message}");
                }
            }

            Console.WriteLine($"✅ İlanburda: Toplam {listings.Count} ilan scrape edildi");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ İlanburda scraping hatası: {ex.Message}");
        }

        return listings;
    }

    private string ExtractIdFromUrl(string url)
    {
        // URL formatı: /slug-1234 veya https://www.ilanburda.net/slug-1234
        try
        {
            var lastDash = url.LastIndexOf('-');
            if (lastDash > 0)
            {
                var idPart = url.Substring(lastDash + 1);
                if (int.TryParse(idPart, out _))
                {
                    return idPart;
                }
            }
        }
        catch { }
        
        return "";
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
