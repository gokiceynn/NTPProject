namespace ListingMonitor.Infrastructure.Scraping;

public class ListingDto
{
    /// <summary>
    /// Kaynak site (youthall, secretcv, microfon)
    /// </summary>
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Site içi benzersiz ID (URL hash veya site ID)
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;
    
    /// <summary>
    /// İlan başlığı
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// İlan açıklaması
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// İlan linki
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// Şirket/Kurum adı
    /// </summary>
    public string Company { get; set; } = string.Empty;
    
    /// <summary>
    /// Lokasyon
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Pozisyon türü (Tam Zamanlı, Stajyer, vb)
    /// </summary>
    public string JobType { get; set; } = string.Empty;
    
    /// <summary>
    /// Fiyat/Burs miktarı
    /// </summary>
    public decimal? Price { get; set; }
    
    /// <summary>
    /// Para birimi
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Şehir
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// Sitede oluşturulma tarihi
    /// </summary>
    public DateTime? CreatedAtOnSite { get; set; }
    
    /// <summary>
    /// İlan tipi (iş, burs, eğitim)
    /// </summary>
    public string ListingType { get; set; } = "job";
}
