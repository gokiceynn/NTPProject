using ListingMonitor.Domain.Entities;

namespace ListingMonitor.Infrastructure.Scraping;

public interface ISiteScraper
{
    Task<IList<ListingDto>> FetchListingsAsync(Site site, SiteParserConfig? config);
}
