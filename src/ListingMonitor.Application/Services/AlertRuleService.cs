using ListingMonitor.Domain.Entities;
using ListingMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ListingMonitor.Application.Services;

public class AlertRuleService
{
    private readonly AppDbContext _context;

    public AlertRuleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AlertRule>> GetAllRulesAsync()
    {
        return await _context.AlertRules
            .Include(r => r.Site)
            .ToListAsync();
    }

    public async Task<AlertRule?> GetRuleByIdAsync(int id)
    {
        return await _context.AlertRules
            .Include(r => r.Site)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<AlertRule>> GetActiveRulesBySiteAsync(int siteId)
    {
        return await _context.AlertRules
            .Where(r => r.SiteId == siteId && r.IsActive)
            .ToListAsync();
    }

    public async Task<AlertRule> CreateRuleAsync(AlertRule rule)
    {
        _context.AlertRules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task AddRuleAsync(AlertRule rule)
    {
        await CreateRuleAsync(rule);
    }

    public async Task UpdateRuleAsync(AlertRule rule)
    {
        _context.AlertRules.Update(rule);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRuleAsync(int id)
    {
        var rule = await _context.AlertRules.FindAsync(id);
        if (rule != null)
        {
            _context.AlertRules.Remove(rule);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<AlertRule>> GetActiveRulesAsync()
    {
        return await _context.AlertRules
            .Include(r => r.Site)
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    public bool DoesListingMatchRule(Listing listing, AlertRule rule)
    {
        // Site kontrolü
        if (rule.SiteId.HasValue && rule.SiteId.Value != listing.SiteId)
        {
            return false;
        }

        // Sadece yeni ilanlar
        if (rule.OnlyNewListings && listing.FirstSeenAt < DateTime.UtcNow.AddMinutes(-5))
        {
            return false;
        }

        // Fiyat kontrolü
        if (rule.MinPrice.HasValue && listing.Price < rule.MinPrice.Value)
        {
            return false;
        }

        if (rule.MaxPrice.HasValue && listing.Price > rule.MaxPrice.Value)
        {
            return false;
        }

        // Şehir kontrolü
        if (!string.IsNullOrEmpty(rule.City) && !string.IsNullOrEmpty(listing.City))
        {
            if (!listing.City.Contains(rule.City, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Anahtar kelime kontrolü
        if (!string.IsNullOrEmpty(rule.Keywords))
        {
            var keywords = rule.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToLowerInvariant());

            var titleLower = listing.Title.ToLowerInvariant();

            var hasMatch = keywords.Any(keyword => titleLower.Contains(keyword));
            if (!hasMatch)
            {
                return false;
            }
        }

        return true;
    }
}
