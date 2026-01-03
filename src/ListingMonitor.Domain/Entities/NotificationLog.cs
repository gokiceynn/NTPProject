using ListingMonitor.Domain.Enums;

namespace ListingMonitor.Domain.Entities;

public class NotificationLog
{
    public int Id { get; set; }
    
    // Nullable - genel log kayıtları için (test mail, ilk çalıştırma vb.)
    public int? RuleId { get; set; }
    public int? ListingId { get; set; }
    
    public string ToEmail { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public NotificationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation properties (nullable)
    public AlertRule? Rule { get; set; }
    public Listing? Listing { get; set; }
}
