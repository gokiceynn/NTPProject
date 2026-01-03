namespace ListingMonitor.Domain.Entities;

public class AlertRule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? SiteId { get; set; } // Nullable - tüm siteler için de olabilir
    
    public string? Keywords { get; set; } // Comma-separated
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? City { get; set; }
    public bool OnlyNewListings { get; set; } = true;
    public string EmailsToNotify { get; set; } = string.Empty; // Comma-separated
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Mail gönderim zamanlaması
    public bool EnableScheduledEmail { get; set; } = false;
    public int? EmailIntervalHours { get; set; } // 1, 6, 12, 24 saat
    public DateTime? LastEmailSentAt { get; set; }
    public DateTime? NextEmailSendAt { get; set; }

    // Navigation properties
    public Site? Site { get; set; }
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}
