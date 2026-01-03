using ListingMonitor.Application.Services;
using ListingMonitor.Infrastructure.Data;
using ListingMonitor.Infrastructure.Email;
using ListingMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListingMonitor.Application.Services;

public class InitialRunEmailService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    
    public InitialRunEmailService(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }
    
    /// <summary>
    /// Tüm sitelerden tüm ilanları mail olarak gönder
    /// </summary>
    public async Task SendAllListingsAsync(string recipientEmail, int? siteId = null)
    {
        try
        {
            Console.WriteLine($"📧 İlan mail gönderimi başlatılıyor... (Site: {(siteId.HasValue ? siteId.Value.ToString() : "Tümü")})");
            
            // İlanları al (siteId null ise tüm siteler)
            var query = _context.Listings
                .Include(l => l.Site)
                .AsQueryable();
            
            if (siteId.HasValue)
            {
                query = query.Where(l => l.SiteId == siteId.Value);
            }
            
            var listings = await query
                .OrderBy(l => l.Site!.Name)
                .ThenByDescending(l => l.FirstSeenAt)
                .ToListAsync();
            
            if (!listings.Any())
            {
                Console.WriteLine("📭 Gönderilecek ilan yok");
                return;
            }
            
            // Site gruplarına göre ilan sayısını hesapla
            var siteGroups = listings.GroupBy(l => l.Site?.Name ?? "Bilinmeyen")
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();
            
            // Email body oluştur
            var emailBody = CreateAllSitesEmailBody(listings);
            var siteNames = siteId.HasValue 
                ? listings.First().Site?.Name ?? "Site" 
                : "Tüm Siteler";
            var subject = $"🎯 İlan Takip Sistemi - {siteNames} ({listings.Count} ilan)";
            
            await _emailService.SendEmailAsync(recipientEmail, subject, emailBody);
            
            Console.WriteLine($"✅ {listings.Count} ilan mail olarak gönderildi");
            Console.WriteLine($"   📧 Gönderilen adres: {recipientEmail}");
            Console.WriteLine($"   📊 Siteler: {string.Join(", ", siteGroups)}");
            
            // Database'e log kaydet
            _context.NotificationLogs.Add(new()
            {
                ToEmail = recipientEmail,
                Status = Domain.Enums.NotificationStatus.Success,
                ErrorMessage = $"Manuel gönderim: {listings.Count} ilan ({siteNames})",
                SentAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Mail gönderim hatası: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Tek site için ilanları mail olarak gönder (eski metod - uyumluluk için)
    /// </summary>
    public async Task SendInitialListingsAsync(int siteId, string recipientEmail)
    {
        try
        {
            Console.WriteLine("📧 İlk çalıştırma - Tüm ilanlar mail olarak gönderiliyor...");
            
            // Mevcut tüm ilanları al
            var listings = await _context.Listings
                .Include(l => l.Site)
                .Where(l => l.SiteId == siteId)
                .OrderByDescending(l => l.FirstSeenAt)
                .ToListAsync();
            
            if (!listings.Any())
            {
                Console.WriteLine("📭 Gönderilecek ilan yok");
                return;
            }
            
            // Email body oluştur (liste formatında)
            var emailBody = CreateInitialEmailBody(listings);
            var subject = $"🎯 İlan Takip Sistemi - {listings.First().Site?.Name} İlanları ({listings.Count} adet)";
            
            // UI'da kaydedilen SMTP ayarlarını kullanarak email gönder
            await _emailService.SendEmailAsync(recipientEmail, subject, emailBody);
            
            Console.WriteLine($"✅ {listings.Count} ilan mail olarak gönderildi");
            Console.WriteLine($"   📧 Gönderilen adres: {recipientEmail}");
            
            // Database'e log kaydet
            _context.NotificationLogs.Add(new()
            {
                ToEmail = recipientEmail,
                Status = Domain.Enums.NotificationStatus.Success,
                ErrorMessage = $"İlk çalıştırma: {listings.Count} ilan gönderildi",
                SentAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ İlk çalıştırma mail hatası: {ex.Message}");
            
            // Hata log'u
            _context.NotificationLogs.Add(new()
            {
                ToEmail = recipientEmail,
                Status = Domain.Enums.NotificationStatus.Failed,
                ErrorMessage = $"İlk çalıştırma hatası: {ex.Message}",
                SentAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
    
    private string CreateAllSitesEmailBody(List<Listing> listings)
    {
        var siteGroups = listings.GroupBy(l => l.Site?.Name ?? "Bilinmeyen").ToList();
        
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>🎯 İlan Takip Sistemi - İlan Raporu</h2>
                <p><strong>Toplam {listings.Count} ilan, {siteGroups.Count} site:</strong></p>
                
                <div style='background-color: #e7f3ff; padding: 10px; border-radius: 5px; margin-bottom: 20px;'>
                    {string.Join(" | ", siteGroups.Select(g => $"<strong>{g.Key}:</strong> {g.Count()}"))}
                </div>";
        
        foreach (var siteGroup in siteGroups)
        {
            body += $@"
                <h3 style='background-color: #007bff; color: white; padding: 10px; border-radius: 5px;'>
                    🌐 {siteGroup.Key} ({siteGroup.Count()} ilan)
                </h3>
                <table style='border-collapse: collapse; width: 100%; margin-bottom: 30px;'>
                    <tr style='background-color: #f2f2f2;'>
                        <th style='border: 1px solid #ddd; padding: 10px; text-align: left; width: 5%;'>#</th>
                        <th style='border: 1px solid #ddd; padding: 10px; text-align: left; width: 50%;'>İlan</th>
                        <th style='border: 1px solid #ddd; padding: 10px; text-align: left; width: 25%;'>Şirket</th>
                        <th style='border: 1px solid #ddd; padding: 10px; text-align: center; width: 20%;'>Link</th>
                    </tr>";
            
            int counter = 1;
            foreach (var listing in siteGroup.Take(100)) // Her siteden max 100 ilan
            {
                body += $@"
                    <tr style='background-color: {(counter % 2 == 0 ? "#f9f9f9" : "white")}'>
                        <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{counter++}</td>
                        <td style='border: 1px solid #ddd; padding: 8px;'><strong>{TruncateText(listing.Title, 60)}</strong></td>
                        <td style='border: 1px solid #ddd; padding: 8px;'>{listing.Company ?? "-"}</td>
                        <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>
                            <a href='{listing.Url}' target='_blank' style='background-color: #28a745; color: white; padding: 5px 10px; text-decoration: none; border-radius: 3px;'>Görüntüle</a>
                        </td>
                    </tr>";
            }
            
            if (siteGroup.Count() > 100)
            {
                body += $@"
                    <tr>
                        <td colspan='4' style='border: 1px solid #ddd; padding: 10px; text-align: center; background-color: #fff3cd;'>
                            <em>... ve {siteGroup.Count() - 100} ilan daha (toplam {siteGroup.Count()})</em>
                        </td>
                    </tr>";
            }
            
            body += "</table>";
        }
        
        body += $@"
                <hr>
                <p style='color: #666;'><small>📅 Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm} | İlan Takip Sistemi v1.0</small></p>
            </body>
            </html>";
        
        return body;
    }
    
    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "-";
        return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
    }
    
    private string CreateInitialEmailBody(List<Listing> listings)
    {
        var body = $@"
            <html>
            <body>
                <h2>🎯 İlan Takip Sistemi - İlk Çalıştırma Raporu</h2>
                <p><strong>Toplam {listings.Count} ilan bulundu:</strong></p>
                
                <table style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif;'>
                    <tr style='background-color: #f2f2f2;'>
                        <th style='border: 1px solid #ddd; padding: 12px; text-align: left;'>#</th>
                        <th style='border: 1px solid #ddd; padding: 12px; text-align: left;'>İlan Başlığı</th>
                        <th style='border: 1px solid #ddd; padding: 12px; text-align: left;'>Şirket</th>
                        <th style='border: 1px solid #ddd; padding: 12px; text-align: left;'>Tarih</th>
                        <th style='border: 1px solid #ddd; padding: 12px; text-align: center;'>Link</th>
                    </tr>";
        
        int counter = 1;
        foreach (var listing in listings) // TÜM ilanları gönder
        {
            body += $@"
                <tr>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center; font-weight: bold;'>{counter++}</td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>
                        <strong>{listing.Title}</strong>
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>
                        {listing.Company ?? listing.Site?.Name ?? "-"}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>
                        {listing.FirstSeenAt:dd.MM.yyyy}
                    </td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>
                        <a href='{listing.Url}' target='_blank' style='background-color: #007bff; color: white; padding: 4px 8px; text-decoration: none; border-radius: 3px; font-size: 12px;'>İlanı Gör</a>
                    </td>
                </tr>";
        }
        
        body += $@"
                </table>
                
                <br>
                <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff;'>
                    <h3>📊 Sistem Bilgileri</h3>
                    <ul>
                        <li><strong>Toplam İlan:</strong> {listings.Count} adet</li>
                        <li><strong>Bu Email'de:</strong> {Math.Min(50, listings.Count)} adet gösteriliyor</li>
                        <li><strong>Site:</strong> {listings.FirstOrDefault()?.Site?.Name}</li>
                        <li><strong>Rapor Tarihi:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</li>
                    </ul>
                </div>
                
                <br>
                <p><em>🚀 Bu sistem 10 dakikada bir yeni ilanları kontrol edecektir.</em></p>
                <p><em>📧 Yeni ilan bulunduğunda otomatik bildirim alacaksınız.</em></p>
                
                <hr>
                <p><small>İlan Takip Sistemi v1.0 | Toplam {listings.Count} ilan</small></p>
            </body>
            </html>
        ";
        
        return body;
    }
}
