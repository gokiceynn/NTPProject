using ListingMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListingMonitor.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Site> Sites { get; set; }
    public DbSet<SiteParserConfig> SiteParserConfigs { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<AlertRule> AlertRules { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Site configuration
        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BaseUrl).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.AutoSiteKey).HasMaxLength(100);
            
            entity.HasOne(e => e.ParserConfig)
                .WithOne(e => e.Site)
                .HasForeignKey<SiteParserConfig>(e => e.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SiteParserConfig configuration
        modelBuilder.Entity<SiteParserConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ListingItemSelector).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TitleSelector).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PriceSelector).IsRequired().HasMaxLength(500);
            entity.Property(e => e.UrlSelector).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DateSelector).HasMaxLength(500);
            entity.Property(e => e.ListingIdSelector).HasMaxLength(500);
            entity.Property(e => e.Encoding).HasMaxLength(50);
        });

        // Listing configuration
        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.City).HasMaxLength(100);
            
            entity.HasIndex(e => new { e.SiteId, e.ExternalId }).IsUnique();
            
            entity.HasOne(e => e.Site)
                .WithMany(e => e.Listings)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AlertRule configuration
        modelBuilder.Entity<AlertRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Keywords).HasMaxLength(1000);
            entity.Property(e => e.MinPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaxPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.EmailsToNotify).IsRequired().HasMaxLength(2000);
            
            entity.HasOne(e => e.Site)
                .WithMany(e => e.AlertRules)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // NotificationLog configuration
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ToEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            
            entity.HasOne(e => e.Rule)
                .WithMany(e => e.NotificationLogs)
                .HasForeignKey(e => e.RuleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Listing)
                .WithMany(e => e.NotificationLogs)
                .HasForeignKey(e => e.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AppSetting configuration
        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(2000);
            
            entity.HasIndex(e => e.Key).IsUnique();
        });
    }
}
