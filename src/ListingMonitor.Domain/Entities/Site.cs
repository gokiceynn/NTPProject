using ListingMonitor.Domain.Enums;

namespace ListingMonitor.Domain.Entities;

public class Site
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public SiteType SiteType { get; set; }
    public string? AutoSiteKey { get; set; }
    public int? CheckIntervalMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public SiteParserConfig? ParserConfig { get; set; }
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
}
