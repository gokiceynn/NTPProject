using Microsoft.EntityFrameworkCore;
using ListingMonitor.Infrastructure.Data;
using ListingMonitor.Domain.Entities;

namespace ListingMonitor.Test;

public class DebugDatabaseContext
{
    public static async Task RunDebugAsync()
    {
        Console.WriteLine("🗄️ DATABASE CONTEXT DEBUG");
        Console.WriteLine("==========================");
        
        try
        {
            // Veritabanı yolunu dinamik olarak belirle
            var basePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
            var dbPath = Path.Combine(basePath, "src", "ListingMonitor.Infrastructure", "listingmonitor.db");
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;
            
            var context1 = new AppDbContext(options);
            var context2 = new AppDbContext(options);
            
            Console.WriteLine("🔍 CONTEXT 1 - Mevcut ilanlar:");
            var count1 = await context1.Listings.CountAsync();
            Console.WriteLine($"   📊 Toplam: {count1}");
            
            Console.WriteLine("🔍 CONTEXT 2 - Mevcut ilanlar:");
            var count2 = await context2.Listings.CountAsync();
            Console.WriteLine($"   📊 Toplam: {count2}");
            
            Console.WriteLine("🔍 ExternalId Test:");
            var externalIds = await context1.Listings
                .Where(l => l.SiteId == 1)
                .Select(l => l.ExternalId)
                .Take(5)
                .ToListAsync();
            
            Console.WriteLine($"   🆔 İlk 5 ExternalId: {string.Join(", ", externalIds)}");
            
            Console.WriteLine("🔍 SiteId Test:");
            var siteIds = await context1.Listings
                .Select(l => l.SiteId)
                .Distinct()
                .ToListAsync();
            
            Console.WriteLine($"   🏢 Mevcut SiteId'ler: {string.Join(", ", siteIds)}");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Hata: {ex.Message}");
        }
        
        Console.WriteLine("\n🏁 Test tamamlandı! Çıkmak için Enter...");
        Console.ReadLine();
    }
}
