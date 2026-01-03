using ListingMonitor.Domain.Entities;
using ListingMonitor.Infrastructure.Scraping;
using ListingMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ListingMonitor.Application.Services;

public class ListingDiffService
{
    private readonly AppDbContext _context;
    
    public ListingDiffService(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Yeni ilanları tespit eder ve sadece yeni olanları döndürür
    /// </summary>
    public async Task<List<ListingDto>> GetNewListingsAsync(int siteId, List<ListingDto> scrapedListings)
    {
        Console.WriteLine("🚀 GetNewListingsAsync BAŞLADI!");
        Console.WriteLine($"🔍 SiteId: {siteId}");
        Console.WriteLine($"📋 Gelen ilan sayısı: {scrapedListings.Count}");
        
        var newListings = new List<ListingDto>();
        
        // Mevcut ilanların ExternalId'lerini al
        Console.WriteLine("🔍 Database sorgusu başlıyor...");
        var existingExternalIds = await _context.Listings
            .Where(l => l.SiteId == siteId)
            .Select(l => l.ExternalId)
            .ToListAsync();
        
        Console.WriteLine($"📋 Mevcut ilan sayısı: {existingExternalIds.Count}");
        Console.WriteLine($"📋 Mevcut ExternalId'ler: {string.Join(", ", existingExternalIds.Take(5))}...");
        
        // Yeni ilanları filtrele
        int duplicateCount = 0;
        Console.WriteLine("🔍 İlan kontrolü başlıyor...");
        
        foreach (var listing in scrapedListings)
        {
            Console.WriteLine($"🔍 İlan kontrol: {listing.Title} | ID: {listing.ExternalId}");
            
            if (!string.IsNullOrWhiteSpace(listing.ExternalId) && 
                !existingExternalIds.Contains(listing.ExternalId))
            {
                newListings.Add(listing);
                Console.WriteLine($"🆕 Yeni ilan: {listing.Title}");
            }
            else
            {
                duplicateCount++;
                Console.WriteLine($"🔄 Duplicate: {listing.Title}");
            }
        }
        
        Console.WriteLine($"📊 Sonuç: {newListings.Count} yeni, {duplicateCount} duplicate");
        Console.WriteLine($"✨ Toplam yeni ilan: {newListings.Count}");
        Console.WriteLine("🏁 GetNewListingsAsync BİTTİ!");
        
        return newListings;
    }
    
    /// <summary>
    /// İlanları veritabanına kaydeder (sadece yeniler)
    /// </summary>
    public async Task SaveNewListingsAsync(int siteId, List<ListingDto> newListings)
    {
        if (!newListings.Any())
        {
            Console.WriteLine("📭 Kaydedilecek yeni ilan yok");
            return;
        }
        
        int successCount = 0;
        int errorCount = 0;
        
        foreach (var dto in newListings)
        {
            try
            {
                var listing = new Listing
                {
                    SiteId = siteId,
                    Title = dto.Title?.Length > 1000 ? dto.Title.Substring(0, 997) + "..." : dto.Title ?? "Başlıksız",
                    Company = dto.Company?.Length > 200 ? dto.Company.Substring(0, 197) + "..." : dto.Company,
                    Url = dto.Url ?? "#",
                    ExternalId = dto.ExternalId ?? "",
                    FirstSeenAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                _context.Listings.Add(listing);
                await _context.SaveChangesAsync();
                successCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ İlan kaydedilemedi: {dto.Title?.Substring(0, Math.Min(50, dto.Title?.Length ?? 0))}...");
                Console.WriteLine($"         Hata: {ex.Message}");
                errorCount++;
                
                // Context'i temizle
                _context.ChangeTracker.Clear();
            }
        }
        
        Console.WriteLine($"💾 {successCount} yeni ilan kaydedildi, {errorCount} hata");
    }
}
