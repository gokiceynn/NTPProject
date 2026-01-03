namespace ListingMonitor.Domain.Entities;

public class Listing
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    
    public string ExternalId { get; set; } = string.Empty; // Site içi ID veya benzersiz string
    public string Title { get; set; } = string.Empty;
    public string? Company { get; set; } // Şirket adı
    public decimal? Price { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? City { get; set; }
    public DateTime? CreatedAtOnSite { get; set; }
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Site Site { get; set; } = null!;
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}
