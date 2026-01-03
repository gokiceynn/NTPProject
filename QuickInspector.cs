using Microsoft.Playwright;

class QuickInspector
{
    static async Task Main()
    {
        Console.WriteLine("🔍 Hızlı Site Inspector");
        Console.WriteLine("========================");
        
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, // Tarayıcıyı gör
            SlowMo = 1000
        });
        
        var page = await browser.NewPageAsync();
        
        // Youthall test
        Console.WriteLine("Youthall açılıyor...");
        await page.GotoAsync("https://www.youthall.com/tr/jobs/");
        
        // Screenshot
        await page.ScreenshotAsync(new PageScreenshotOptions 
        { 
            Path = "youthall_check.png",
            FullPage = true 
        });
        
        Console.WriteLine("Screenshot alındı: youthall_check.png");
        Console.WriteLine("Tarayıcı açık kalacak - DOM'u inceleyebilirsiniz");
        Console.WriteLine("Kapatmak için Enter tuşuna bas...");
        Console.ReadLine();
        
        await browser.CloseAsync();
        playwright.Dispose();
    }
}
