using System.Text;
using ListingMonitor.Domain.Entities;
using ListingMonitor.Domain.Enums;
using ListingMonitor.Infrastructure.Data;
using ListingMonitor.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

namespace ListingMonitor.Application.Services;

public class NotificationService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public NotificationService(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task SendListingNotificationAsync(AlertRule rule, Listing listing)
    {
        var emails = rule.EmailsToNotify.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .Where(e => !string.IsNullOrEmpty(e));

        foreach (var email in emails)
        {
            try
            {
                var subject = $"İlan takip uygulaması {DateTime.Now:dd.MM.yyyy HH:mm}";
                var body = GenerateEmailBody(rule, listing);

                await _emailService.SendEmailAsync(email, subject, body);

                // Başarılı log
                await LogNotificationAsync(rule.Id, listing.Id, email, NotificationStatus.Success, null);
            }
            catch (Exception ex)
            {
                // Hata log
                await LogNotificationAsync(rule.Id, listing.Id, email, NotificationStatus.Failed, ex.Message);
            }
        }
    }

    private async Task LogNotificationAsync(int ruleId, int listingId, string email, 
        NotificationStatus status, string? errorMessage)
    {
        var log = new NotificationLog
        {
            RuleId = ruleId,
            ListingId = listingId,
            ToEmail = email,
            Status = status,
            ErrorMessage = errorMessage,
            SentAt = DateTime.UtcNow
        };

        _context.NotificationLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    private string GenerateEmailBody(AlertRule rule, Listing listing)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body style='font-family: Arial, sans-serif;'>");
        sb.AppendLine("<h2>yeni düşen ilanlar</h2>");
        sb.AppendLine("<hr/>");
        sb.AppendLine($"<p><strong>Başlık:</strong> {listing.Title}</p>");
        
        if (listing.Price.HasValue)
        {
            sb.AppendLine($"<p><strong>Fiyat:</strong> {listing.Price:N2} TL</p>");
        }
        
        if (!string.IsNullOrEmpty(listing.City))
        {
            sb.AppendLine($"<p><strong>Şehir:</strong> {listing.City}</p>");
        }
        
        sb.AppendLine($"<p><strong>Kural:</strong> {rule.Name}</p>");
        sb.AppendLine($"<p><strong>Link:</strong> <a href='{listing.Url}'>{listing.Url}</a></p>");
        sb.AppendLine("<hr/>");
        sb.AppendLine($"<p style='color: gray; font-size: 12px;'>Bu mail {DateTime.Now:dd.MM.yyyy HH:mm} tarihinde otomatik gönderilmiştir.</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    public async Task SendEmailAsync(string subject, string body, string? to = null)
    {
        if (string.IsNullOrEmpty(to))
        {
            // SMTP ayarlarından varsayılan email'i al
            var smtpSettings = await _context.AppSettings
                .Where(s => s.Key == "FromEmail")
                .FirstOrDefaultAsync();
            
            to = smtpSettings?.Value ?? "default@example.com";
        }

        await _emailService.SendEmailAsync(to, subject, body);
    }

    public async Task<List<NotificationLog>> GetRecentNotificationsAsync(int count = 100)
    {
        return await _context.NotificationLogs
            .Include(n => n.Rule)
            .Include(n => n.Listing)
            .OrderByDescending(n => n.SentAt)
            .Take(count)
            .ToListAsync();
    }
}
