using ListingMonitor.Domain.Enums;

namespace ListingMonitor.Domain.Entities;

public class SiteParserConfig
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    
    // HTML seçiciler
    public string ListingItemSelector { get; set; } = string.Empty;
    public string TitleSelector { get; set; } = string.Empty;
    public string PriceSelector { get; set; } = string.Empty;
    public string UrlSelector { get; set; } = string.Empty;
    public string? DateSelector { get; set; }
    public string? ListingIdSelector { get; set; }
    
    public SelectorType SelectorType { get; set; } = SelectorType.Css;
    public string Encoding { get; set; } = "utf-8";

    // Navigation property
    public Site Site { get; set; } = null!;
}
