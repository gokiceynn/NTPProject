using Microsoft.Playwright;

namespace ListingMonitor.Test;

public class SiteInspector
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🔍 Site DOM Inspector");
        Console.WriteLine("=====================");
        
        var sites = new[]
        {
            new { Name = "Youthall", Url = "https://www.youthall.com/tr/jobs/" },
            new { Name = "Secretcv", Url = "https://www.secretcv.com/is-ilanlari" },
            new { Name = "Microfon", Url = "https://microfon.co/en/scholarship?level=abroad" }
        };
        
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, // Görmek için
            SlowMo = 500 // Yavaş çalışsın
        });
        
        foreach (var site in sites)
        {
            try
            {
                Console.WriteLine($"\n🌐 {site.Name} inceleniyor...");
                var page = await browser.NewPageAsync();
                
                // Siteyi aç
                await page.GotoAsync(site.Url, new PageGotoOptions 
                { 
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 60000 
                });
                
                // Screenshot al
                await page.ScreenshotAsync(new PageScreenshotOptions 
                { 
                    Path = $"{site.Name.ToLower()}_screenshot.png",
                    FullPage = true 
                });
                
                // HTML'i kaydet
                var html = await page.ContentAsync();
                await File.WriteAllTextAsync($"{site.Name.ToLower()}_dom.html", html);
                
                // Tüm linkleri analiz et
                var allLinks = await page.QuerySelectorAllAsync("a");
                var relevantLinks = new List<string>();
                
                foreach (var link in allLinks.Take(50)) // İlk 50 link
                {
                    var href = await link.GetAttributeAsync("href");
                    var text = await link.TextContentAsync();
                    
                    if (!string.IsNullOrWhiteSpace(href) && 
                        !string.IsNullOrWhiteSpace(text))
                    {
                        relevantLinks.Add($"HREF: {href}\nTEXT: {text?.Trim()}\n---");
                    }
                }
                
                // Linkleri kaydet
                await File.WriteAllTextAsync($"{site.Name.ToLower()}_links.txt", 
                    $"Total links found: {allLinks.Count}\n\n" + 
                    string.Join("\n", relevantLinks));
                
                Console.WriteLine($"   ✅ {site.Name} analiz edildi");
                Console.WriteLine($"   📊 {allLinks.Count} link bulundu");
                Console.WriteLine($"   💾 Dosyalar kaydedildi:");
                Console.WriteLine($"      - {site.Name.ToLower()}_screenshot.png");
                Console.WriteLine($"      - {site.Name.ToLower()}_dom.html");
                Console.WriteLine($"      - {site.Name.ToLower()}_links.txt");
                
                await page.CloseAsync();
                
                // Kullanıcı devam etmek ister mi?
                Console.WriteLine("\nDevam etmek için Enter tuşuna bas...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ {site.Name} hatası: {ex.Message}");
            }
        }
        
        await browser.CloseAsync();
        playwright.Dispose();
        
        Console.WriteLine("\n🎯 Analiz tamamlandı!");
        Console.WriteLine("Şimdi bu dosyaları inceleyip doğru XPath selector'larını bulabilirsiniz:");
        Console.WriteLine("1. Screenshot'lar - Görsel yapı");
        Console.WriteLine("2. HTML dosyaları - DOM structure");
        Console.WriteLine("3. Links dosyaları - İlan linkleri");
    }
}
