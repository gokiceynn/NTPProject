using ListingMonitor.Infrastructure.Scraping;
using ListingMonitor.Infrastructure.Scraping.Adapters;

namespace ListingMonitor.Test;

public class ProgramMain
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Modern Scraping Test");
        Console.WriteLine("=====================================");
        
        using var scraper = new ModernScrapingService();
        
        try
        {
            // Test adapters
            Console.WriteLine("\n📡 Adapter testleri...");
            
            var adapters = scraper.GetAvailableAdapters();
            Console.WriteLine($"   Mevcut adapterlar: {string.Join(", ", adapters)}");
            
            // Youthall test
            Console.WriteLine("\n🔍 Youthall test...");
            var youthallAvailable = await scraper.TestAdapterAsync("youthall");
            Console.WriteLine($"   Youthall erişilebilir: {(youthallAvailable ? "✅ Evet" : "❌ Hayır")}");
            
            // Microfon test
            Console.WriteLine("\n🔍 Microfon test...");
            var microfonAvailable = await scraper.TestAdapterAsync("microfon");
            Console.WriteLine($"   Microfon erişilebilir: {(microfonAvailable ? "✅ Evet" : "❌ Hayır")}");
            
            // Bursverenler test
            Console.WriteLine("\n🔍 Bursverenler test...");
            var bursverenlerAvailable = await scraper.TestAdapterAsync("bursverenler");
            Console.WriteLine($"   Bursverenler erişilebilir: {(bursverenlerAvailable ? "✅ Evet" : "❌ Hayır")}");
            
            // İlanburda test
            Console.WriteLine("\n🔍 İlanburda test...");
            var ilanburdaAvailable = await scraper.TestAdapterAsync("ilanburda");
            Console.WriteLine($"   İlanburda erişilebilir: {(ilanburdaAvailable ? "✅ Evet" : "❌ Hayır")}");
            
            Console.WriteLine("\n✅ Test tamamlandı!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Test sırasında hata: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
        }
        
        Console.WriteLine("\nTest tamamlandı.");
    }
}
