using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ListingMonitor.Domain.Entities;
using ListingMonitor.Domain.Enums;

namespace ListingMonitor.Infrastructure.Scraping;

public class ManualSiteScraper : ISiteScraper
{
    private readonly HttpClient _httpClient;

    public ManualSiteScraper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IList<ListingDto>> FetchListingsAsync(Site site, SiteParserConfig? config)
    {
        if (config == null)
        {
            throw new ArgumentException("Manuel site için parser config gereklidir", nameof(config));
        }

        var listings = new List<ListingDto>();
        var currentPage = 1;
        var maxPages = 25; // Güvenlik sınırı - sonsuz döngüyü önlemek için

        try
        {
            while (currentPage <= maxPages)
            {
                Console.WriteLine($"      Sayfa {currentPage} çekiliyor...");
                
                // Sayfa URL'ini oluştur
                var pageUrl = GetPageUrl(site.BaseUrl, currentPage);
                
                // HTML sayfasını çek
                var html = await _httpClient.GetStringAsync(pageUrl);
                
                // Encoding ayarla
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // İlan kartlarını bul (sadece XPath kullan - HtmlAgilityPack CSS selector desteklemiyor)
                var selector = config.SelectorType == SelectorType.Css 
                    ? ConvertCssToXPath(config.ListingItemSelector) 
                    : config.ListingItemSelector;
                
                var listingNodes = htmlDoc.DocumentNode.SelectNodes(selector);

                if (listingNodes == null || listingNodes.Count == 0)
                {
                    Console.WriteLine($"      Sayfa {currentPage}: İlan bulunamadı, scraping durduruluyor.");
                    break;
                }

                var pageListings = new List<ListingDto>();
                foreach (var node in listingNodes)
                {
                    try
                    {
                        var url = ExtractUrl(node, config.UrlSelector, config.SelectorType, site.BaseUrl);
                        var title = CleanText(ExtractText(node, config.TitleSelector, config.SelectorType));
                        
                        // URL veya Title yoksa atla
                        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(title))
                            continue;
                        
                        var listing = new ListingDto
                        {
                            Title = title,
                            Url = url,
                            // ExternalId: URL'den oluştur (daha güvenilir)
                            ExternalId = GenerateIdFromUrl(url)
                        };

                        // Fiyat parse et
                        var priceText = ExtractText(node, config.PriceSelector, config.SelectorType);
                        listing.Price = ParsePrice(priceText);

                        pageListings.Add(listing);
                    }
                    catch (Exception ex)
                    {
                        // Log individual item errors but continue processing
                        Console.WriteLine($"İlan parse edilirken hata: {ex.Message}");
                    }
                }
                
                if (pageListings.Count == 0)
                {
                    Console.WriteLine($"      Sayfa {currentPage}: Geçerli ilan bulunamadı, scraping durduruluyor.");
                    break;
                }
                
                listings.AddRange(pageListings);
                Console.WriteLine($"      Sayfa {currentPage}: {pageListings.Count} ilan bulundu (Toplam: {listings.Count})");
                
                // Sayfalama kontrolü - sonraki sayfa var mı?
                if (!HasNextPage(htmlDoc, currentPage))
                {
                    Console.WriteLine($"      Son sayfa ulaşıldı: {currentPage}");
                    break;
                }
                
                currentPage++;
                
                // Rate limiting - site yükünü azaltmak için bekle
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Site scraping hatası: {ex.Message}");
            throw;
        }

        Console.WriteLine($"      Toplam {listings.Count} ilan çekildi");
        return listings;
    }
    
    private string GetPageUrl(string baseUrl, int pageNumber)
    {
        if (pageNumber == 1)
            return baseUrl;
        
        // Eleman.net için sayfalama formatı: ?sy=2
        if (baseUrl.Contains("eleman.net"))
        {
            if (baseUrl.Contains("?"))
                return $"{baseUrl}&sy={pageNumber}";
            else
                return $"{baseUrl}?sy={pageNumber}";
        }
        
        // Microfon.co için sayfalama formatı: ?page=2
        if (baseUrl.Contains("microfon.co"))
        {
            if (baseUrl.Contains("?"))
                return $"{baseUrl}&page={pageNumber}";
            else
                return $"{baseUrl}?page={pageNumber}";
        }
        
        // Genel sayfalama formatları
        if (baseUrl.Contains("?"))
            return $"{baseUrl}&page={pageNumber}";
        else
            return $"{baseUrl}?page={pageNumber}";
    }
    
    private bool HasNextPage(HtmlDocument htmlDoc, int currentPage)
    {
        // Eleman.net için sy= parametresi ile sayfalama kontrolü
        var syPageLinks = htmlDoc.DocumentNode.SelectNodes("//a[contains(@href, 'sy=')]");
        if (syPageLinks != null)
        {
            var pageNumbers = syPageLinks
                .Select(a => a.GetAttributeValue("href", ""))
                .Where(href => href.Contains("sy="))
                .Select(href =>
                {
                    var match = Regex.Match(href, @"sy=(\d+)");
                    return match.Success ? int.Parse(match.Groups[1].Value) : 0;
                })
                .Where(p => p > 0)
                .ToList();
                
            if (pageNumbers.Any() && pageNumbers.Max() > currentPage)
            {
                return true;
            }
        }
        
        // Genel page= parametresi ile sayfalama kontrolü
        var pageLinks = htmlDoc.DocumentNode.SelectNodes("//a[contains(@href, 'page=')]");
        if (pageLinks != null)
        {
            var pageNumbers = pageLinks
                .Select(a => a.GetAttributeValue("href", ""))
                .Where(href => href.Contains("page="))
                .Select(href =>
                {
                    var match = Regex.Match(href, @"page=(\d+)");
                    return match.Success ? int.Parse(match.Groups[1].Value) : 0;
                })
                .Where(p => p > 0)
                .ToList();
                
            if (pageNumbers.Any() && pageNumbers.Max() > currentPage)
            {
                return true;
            }
        }
        
        // "Sonraki" veya "Next" linki kontrolü
        var nextLinks = htmlDoc.DocumentNode.SelectNodes("//a[contains(translate(text(), 'NEXT', 'next'), 'next')] | //a[contains(text(), 'Sonraki')] | //a[contains(@class, 'next')]");
        if (nextLinks != null && nextLinks.Any())
        {
            return true;
        }
        
        return false;
    }

    private string ExtractText(HtmlNode node, string? selector, SelectorType selectorType)
    {
        if (string.IsNullOrEmpty(selector)) return string.Empty;

        try
        {
            var xpath = selectorType == SelectorType.Css ? ConvertCssToXPath(selector) : selector;
            var targetNode = node.SelectSingleNode(xpath);
            return targetNode?.InnerText?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractUrl(HtmlNode node, string selector, SelectorType selectorType, string baseUrl)
    {
        try
        {
            var xpath = selectorType == SelectorType.Css ? ConvertCssToXPath(selector) : selector;
            var targetNode = node.SelectSingleNode(xpath);

            if (targetNode == null) return string.Empty;

            var href = targetNode.GetAttributeValue("href", string.Empty);
            
            // Relative URL'i absolute yap
            if (!string.IsNullOrEmpty(href) && !href.StartsWith("http"))
            {
                var baseUri = new Uri(baseUrl);
                var absoluteUri = new Uri(baseUri, href);
                return absoluteUri.ToString();
            }

            return href ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private decimal? ParsePrice(string priceText)
    {
        if (string.IsNullOrEmpty(priceText)) return null;

        try
        {
            // Sadece rakamları ve nokta/virgül al
            var cleaned = Regex.Replace(priceText, @"[^\d,.]", "");
            cleaned = cleaned.Replace(".", "").Replace(",", ".");

            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            {
                return price;
            }
        }
        catch
        {
            // Parse hatası
        }

        return null;
    }

    private string GenerateIdFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return Guid.NewGuid().ToString();

        // URL'den ID çıkarmayı dene (örn: is-ilani/teknik-personel-i4555881 -> i4555881)
        var match = Regex.Match(url, @"-i(\d+)$");
        if (match.Success)
        {
            return $"eleman_{match.Groups[1].Value}";
        }
        
        // Alternatif: hash oluştur
        var hash = url.GetHashCode();
        return $"url_{Math.Abs(hash)}";
    }
    
    private string CleanText(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        
        // Birden fazla boşluğu tek boşluğa dönüştür
        text = Regex.Replace(text, @"\s+", " ");
        
        // Baş ve sondaki boşlukları temizle
        text = text.Trim();
        
        // Maksimum 500 karakter (veritabanı güvenliği)
        if (text.Length > 500)
        {
            text = text.Substring(0, 497) + "...";
        }
        
        return text;
    }

    // Basit CSS -> XPath dönüşümü (sadece temel selector'ler için)
    private string ConvertCssToXPath(string cssSelector)
    {
        // Bu basit bir implementasyon. Gerçek projede daha kapsamlı bir kütüphane kullanılmalı
        // Şimdilik sadece basit sınıf ve tag seçicilerini destekleyelim
        
        if (cssSelector.StartsWith("."))
        {
            // .classname -> //*[contains(@class, 'classname')]
            var className = cssSelector.Substring(1);
            return $"//*[contains(@class, '{className}')]";
        }
        else if (cssSelector.StartsWith("#"))
        {
            // #id -> //*[@id='id']
            var id = cssSelector.Substring(1);
            return $"//*[@id='{id}']";
        }
        else if (!cssSelector.Contains("[") && !cssSelector.Contains(">"))
        {
            // tagname -> //tagname
            return $"//{cssSelector}";
        }

        // Daha karmaşık selector'ler için kullanıcının XPath kullanmasını öneriyoruz
        return $"//*[contains(@class, '{cssSelector}')]";
    }
}

