using ListingMonitor.Domain.Entities;
using ListingMonitor.Domain.Enums;
using ListingMonitor.Infrastructure.Data;
using ListingMonitor.Infrastructure.Scraping;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace ListingMonitor.Application.Services;

public class SiteService
{
    private readonly AppDbContext _context;
    private readonly ISiteScraper _modernScraper;
    private readonly HttpClient _httpClient;

    public SiteService(AppDbContext context, ISiteScraper modernScraper, HttpClient httpClient)
    {
        _context = context;
        _modernScraper = modernScraper;
        _httpClient = httpClient;
    }
    
    // Backward compatibility constructor
    public SiteService(AppDbContext context) : this(context, null!, null!)
    {
    }

    public async Task<List<Site>> GetAllSitesAsync()
    {
        return await _context.Sites
            .Include(s => s.ParserConfig)
            .ToListAsync();
    }

    public async Task AddSiteAsync(Site site)
    {
        _context.Sites.Add(site);
        await _context.SaveChangesAsync();
    }

    public async Task<Site?> GetSiteByIdAsync(int id)
    {
        return await _context.Sites
            .Include(s => s.ParserConfig)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Site> CreateSiteAsync(Site site)
    {
        _context.Sites.Add(site);
        await _context.SaveChangesAsync();
        return site;
    }

    public async Task UpdateSiteAsync(Site site)
    {
        site.UpdatedAt = DateTime.UtcNow;
        _context.Sites.Update(site);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSiteAsync(int id)
    {
        var site = await _context.Sites.FindAsync(id);
        if (site != null)
        {
            _context.Sites.Remove(site);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Site>> GetActiveSitesAsync()
    {
        return await _context.Sites
            .Include(s => s.ParserConfig)
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public async Task<List<Listing>> ScrapeSiteAsync(int siteId)
    {
        try
        {
            // Site bilgilerini al
            var site = await _context.Sites
                .Include(s => s.ParserConfig)
                .FirstOrDefaultAsync(s => s.Id == siteId);
            
            if (site == null)
            {
                throw new Exception($"Site bulunamadı: ID={siteId}");
            }

            Console.WriteLine($"🔄 {site.Name} için scraping başlatıldı...");
            
            IList<ListingDto> scrapedListings;
            
            // Site tipine göre scraper seç
            if (site.SiteType == SiteType.AutoSupported)
            {
                // Modern scraper (Youthall, Microfon, İlanburda)
                Console.WriteLine($"   📡 AutoSupported mod - Modern scraper kullanılıyor");
                scrapedListings = await _modernScraper.FetchListingsAsync(site, site.ParserConfig);
            }
            else
            {
                // Manual scraper (XPath/CSS selector)
                Console.WriteLine($"   📝 Manual mod - XPath scraper kullanılıyor");
                if (site.ParserConfig == null)
                {
                    Console.WriteLine($"   ⚠️ ParserConfig bulunamadı!");
                    return new List<Listing>();
                }
                
                var manualScraper = new ManualSiteScraper(_httpClient);
                scrapedListings = await manualScraper.FetchListingsAsync(site, site.ParserConfig);
            }
            
            Console.WriteLine($"   ✅ {scrapedListings.Count} ilan bulundu");
            
            // DTO'ları Listing entity'lerine dönüştür ve kaydet
            var newListings = new List<Listing>();
            foreach (var dto in scrapedListings)
            {
                // Duplicate kontrolü
                var exists = await _context.Listings
                    .AnyAsync(l => l.SiteId == siteId && l.ExternalId == dto.ExternalId);
                
                if (!exists)
                {
                    var listing = new Listing
                    {
                        SiteId = siteId,
                        ExternalId = dto.ExternalId,
                        Title = dto.Title,
                        Url = dto.Url,
                        Price = dto.Price,
                        City = dto.City,
                        Company = dto.Company,
                        FirstSeenAt = DateTime.UtcNow,
                        LastSeenAt = DateTime.UtcNow
                    };
                    
                    _context.Listings.Add(listing);
                    newListings.Add(listing);
                }
            }
            
            if (newListings.Count > 0)
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"   💾 {newListings.Count} yeni ilan kaydedildi");
            }
            else
            {
                Console.WriteLine($"   ℹ️ Yeni ilan yok (tümü zaten mevcut)");
            }
            
            return newListings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Scraping hatası: {ex.Message}");
            throw new Exception($"Scraping hatası: {ex.Message}", ex);
        }
    }
}
