using ListingMonitor.Infrastructure.Scraping;
using ListingMonitor.Infrastructure.Scraping.Adapters;

namespace ListingMonitor.Test;

/// <summary>
/// Simple adapter test - kullanmak için Program.cs'deki Main'i çağırın
/// </summary>
public class SimpleTestMain
{
    public static async Task RunAsync()
    {
        Console.WriteLine("🔍 Simple Adapter Test");
        Console.WriteLine("================================");
        
        try
        {
            // Youthall Adapter Test
            Console.WriteLine("\n🌐 Youthall Adapter Test...");
            using var youthallAdapter = new YouthallAdapter();
            
            var isAvailable = await youthallAdapter.IsAvailableAsync();
            Console.WriteLine($"   Site erişilebilir: {(isAvailable ? "✅ Evet" : "❌ Hayır")}");
            
            if (isAvailable)
            {
                var listings = await youthallAdapter.ScrapeAsync();
                Console.WriteLine($"   Bulunan ilan sayısı: {listings.Count}");
                
                if (listings.Any())
                {
                    Console.WriteLine("\n   İlk 5 ilan:");
                    foreach (var listing in listings.Take(5))
                    {
                        Console.WriteLine($"   • {listing.Title}");
                        Console.WriteLine($"     Şirket: {listing.Company}");
                        Console.WriteLine($"     URL: {listing.Url}");
                        Console.WriteLine();
                    }
                }
            }
            
            // Microfon Adapter Test
            Console.WriteLine("\n🎤 Microfon Adapter Test...");
            using var microfonAdapter = new MicrofonAdapter();
            
            var microfonAvailable = await microfonAdapter.IsAvailableAsync();
            Console.WriteLine($"   Site erişilebilir: {(microfonAvailable ? "✅ Evet" : "❌ Hayır")}");
            
            if (microfonAvailable)
            {
                var microfonListings = await microfonAdapter.ScrapeAsync();
                Console.WriteLine($"   Bulunan ilan sayısı: {microfonListings.Count}");
                
                if (microfonListings.Any())
                {
                    Console.WriteLine("\n   İlk 3 ilan:");
                    foreach (var listing in microfonListings.Take(3))
                    {
                        Console.WriteLine($"   • {listing.Title}");
                        Console.WriteLine($"     Kurum: {listing.Company}");
                        Console.WriteLine();
                    }
                }
            }
            
            Console.WriteLine("\n✅ Test tamamlandı!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Hata: {ex.Message}");
            Console.WriteLine($"   {ex.StackTrace}");
        }
    }
}
