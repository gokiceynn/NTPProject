using System;
using System.Threading.Tasks;
using ListingMonitor.Infrastructure.Scraping.Adapters;

namespace QuickInspector;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Eleman.net manuel test
        if (args.Length > 0 && args[0].ToLower() == "eleman")
        {
            await TestEleman.TestMethod();
            return;
        }
        
        Console.WriteLine("🔍 Adapter Test Başlıyor...\n");
        
        // Test edilecek adapter seç
        var testAll = args.Length == 0 || args[0].ToLower() == "all";
        var testYouthall = testAll || args.Contains("youthall", StringComparer.OrdinalIgnoreCase);
        var testMicrofon = testAll || args.Contains("microfon", StringComparer.OrdinalIgnoreCase);
        var testIlanburda = testAll || args.Contains("ilanburda", StringComparer.OrdinalIgnoreCase);
        
        // 1. Youthall Test
        if (testYouthall)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("🎯 YOUTHALL TEST");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            try
            {
                using var adapter = new YouthallAdapter();
                var listings = await adapter.ScrapeAsync();
                
                Console.WriteLine($"\n✅ Youthall: {listings.Count} ilan bulundu\n");
                
                foreach (var listing in listings.Take(5))
                {
                    Console.WriteLine($"  📌 {listing.Title}");
                    Console.WriteLine($"     🏢 {listing.Company}");
                    Console.WriteLine($"     📍 {listing.City}");
                    Console.WriteLine($"     🔗 {listing.Url}");
                    Console.WriteLine();
                }
                
                if (listings.Count > 5)
                {
                    Console.WriteLine($"  ... ve {listings.Count - 5} ilan daha\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Youthall hatası: {ex.Message}\n");
            }
        }
        
        // 2. Microfon Test
        if (testMicrofon)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("🎯 MICROFON TEST");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            try
            {
                using var adapter = new MicrofonAdapter();
                var listings = await adapter.ScrapeAsync();
                
                Console.WriteLine($"\n✅ Microfon: {listings.Count} burs bulundu\n");
                
                foreach (var listing in listings.Take(5))
                {
                    Console.WriteLine($"  📌 {listing.Title}");
                    Console.WriteLine($"     🏢 {listing.Company}");
                    Console.WriteLine($"     📝 {listing.Description?.Substring(0, Math.Min(listing.Description?.Length ?? 0, 80))}...");
                    Console.WriteLine($"     🔗 {listing.Url}");
                    Console.WriteLine();
                }
                
                if (listings.Count > 5)
                {
                    Console.WriteLine($"  ... ve {listings.Count - 5} burs daha\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Microfon hatası: {ex.Message}\n");
            }
        }
        
        // 3. İlanburda Test
        if (testIlanburda)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("🎯 İLANBURDA TEST");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            try
            {
                using var adapter = new IlanburdaAdapter();
                var listings = await adapter.ScrapeAsync();
                
                Console.WriteLine($"\n✅ İlanburda: {listings.Count} ilan bulundu\n");
                
                foreach (var listing in listings.Take(5))
                {
                    Console.WriteLine($"  📌 {listing.Title}");
                    Console.WriteLine($"     📍 {listing.City}");
                    Console.WriteLine($"     💰 {(listing.Price.HasValue ? listing.Price.Value.ToString("N0") + " TL" : "Belirtilmemiş")}");
                    Console.WriteLine($"     🔗 {listing.Url}");
                    Console.WriteLine();
                }
                
                if (listings.Count > 5)
                {
                    Console.WriteLine($"  ... ve {listings.Count - 5} ilan daha\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ İlanburda hatası: {ex.Message}\n");
            }
        }
        
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("🏁 Test Tamamlandı!");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
}
