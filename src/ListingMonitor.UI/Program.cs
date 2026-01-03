using Avalonia;
using System;
using System.Net.Http;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using ListingMonitor.Infrastructure.Data;
using ListingMonitor.Infrastructure.Email;
using ListingMonitor.Application.Services;
using ListingMonitor.Domain.Entities;
using ListingMonitor.Infrastructure.Scraping;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace ListingMonitor.UI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .AfterSetup(_ => ConfigureServices());

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Database - use singleton for desktop app (one user)
        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ListingMonitor");
        Directory.CreateDirectory(appDataDir);
        var dbPath = Path.Combine(appDataDir, "listingmonitor.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        
        var context = new AppDbContext(options);
        
        // Veritabanı tablolarını oluştur
        context.Database.EnsureCreated();
        Console.WriteLine("✅ Veritabanı tabloları hazır");
        
        services.AddSingleton(context);

        // Infrastructure
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        services.AddSingleton(httpClient);
        services.AddSingleton<ISiteScraper, ModernScrapingService>();
        services.AddSingleton<ListingDiffService>();
        
        // Email service - Dinamik SMTP ayarları için
        var smtpSettings = new SmtpSettings();
        services.AddSingleton(smtpSettings);
        
        // Database'den SMTP ayarlarını yükle
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000); // UI açılsın diye bekle
            await LoadSmtpSettingsFromDatabaseAsync(smtpSettings);
        });
        
        services.AddSingleton<IEmailService>(sp => 
        {
            var settings = sp.GetRequiredService<SmtpSettings>();
            return new SmtpEmailService(settings);
        });
        
        // Application Services
        services.AddSingleton<SiteService>(sp => 
        {
            var ctx = sp.GetRequiredService<AppDbContext>();
            var scraper = sp.GetRequiredService<ISiteScraper>();
            var http = sp.GetRequiredService<HttpClient>();
            return new SiteService(ctx, scraper, http);
        });
        services.AddSingleton<AlertRuleService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<ScraperSchedulerService>(sp => 
        {
            var ctx = sp.GetRequiredService<AppDbContext>();
            var siteService = sp.GetRequiredService<SiteService>();
            var ruleService = sp.GetRequiredService<AlertRuleService>();
            var notificationService = sp.GetRequiredService<NotificationService>();
            var scraper = sp.GetRequiredService<ISiteScraper>();
            var diffService = sp.GetRequiredService<ListingDiffService>();
            var http = sp.GetRequiredService<HttpClient>();
            return new ScraperSchedulerService(ctx, siteService, ruleService, notificationService, scraper, diffService, http);
        });
        services.AddSingleton<InitialRunEmailService>();
        services.AddSingleton<DatabaseBackupService>();

        ServiceLocator.Services = services.BuildServiceProvider();
        
        // İlk başlatma setup'ı
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000); // UI açılsın diye bekle
                await InitializeDefaultDataAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ InitializeDefaultData HATA: {ex.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            }
        });
    }
    
    private static async Task InitializeDefaultDataAsync()
    {
        try
        {
            Console.WriteLine("🚀 İlk başlatma setup'ı başlıyor...");
            
            var context = ServiceLocator.GetService<AppDbContext>();
            var siteService = ServiceLocator.GetService<SiteService>();
            
            var existingSites = await context.Sites.ToListAsync();
            
            // Sadece Youthall ve İlanburda otomatik ekleniyor
            // Microfon için bkz: SITE_EKLEME_REHBERI.md
            var defaultSites = new[]
            {
                new { Name = "Youthall", Url = "https://www.youthall.com/tr/talent-programs/" },
                new { Name = "İlanburda", Url = "https://www.ilanburda.net/8/is-ilanlari" }
            };
            
            foreach (var site in defaultSites)
            {
                if (!existingSites.Any(s => s.Name == site.Name))
                {
                    Console.WriteLine($"🌐 {site.Name} sitesi ekleniyor...");
                    var newSite = new ListingMonitor.Domain.Entities.Site
                    {
                        Name = site.Name,
                        BaseUrl = site.Url,
                        SiteType = ListingMonitor.Domain.Enums.SiteType.AutoSupported,
                        IsActive = true
                    };
                    await siteService.AddSiteAsync(newSite);
                    Console.WriteLine($"✅ {site.Name} sitesi eklendi");
                }
                else
                {
                    Console.WriteLine($"✅ {site.Name} sitesi zaten mevcut");
                }
            }
            
            Console.WriteLine("🎉 İlk başlatma setup'ı tamamlandı!");
            Console.WriteLine("📖 Microfon eklemek için: SITE_EKLEME_REHBERI.md");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Setup hatası: {ex.Message}");
        }
    }
    
    private static async Task LoadSmtpSettingsFromDatabaseAsync(SmtpSettings smtpSettings)
    {
        try
        {
            Console.WriteLine("📧 SMTP ayarları database'den yükleniyor...");
            
            var context = ServiceLocator.GetService<AppDbContext>();
            if (context != null)
            {
                var settings = await context.AppSettings.ToListAsync();
                
                smtpSettings.SmtpHost = settings.FirstOrDefault(s => s.Key == "SmtpHost")?.Value ?? "";
                smtpSettings.SmtpPort = int.Parse(settings.FirstOrDefault(s => s.Key == "SmtpPort")?.Value ?? "587");
                smtpSettings.UseStartTls = bool.Parse(settings.FirstOrDefault(s => s.Key == "UseStartTls")?.Value ?? "true");
                smtpSettings.Username = settings.FirstOrDefault(s => s.Key == "SmtpUsername")?.Value ?? "";
                smtpSettings.Password = settings.FirstOrDefault(s => s.Key == "SmtpPassword")?.Value ?? "";
                smtpSettings.FromEmail = settings.FirstOrDefault(s => s.Key == "FromEmail")?.Value ?? "";
                smtpSettings.FromName = settings.FirstOrDefault(s => s.Key == "FromName")?.Value ?? "";
                
                Console.WriteLine($"✅ SMTP ayarları yüklendi:");
                Console.WriteLine($"   🌐 Host: {smtpSettings.SmtpHost}");
                Console.WriteLine($"   👤 Username: {smtpSettings.Username}");
                Console.WriteLine($"   📤 FromEmail: {smtpSettings.FromEmail}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SMTP yükleme hatası: {ex.Message}");
        }
    }
}

public static class ServiceLocator
{
    public static IServiceProvider? Services { get; set; }
    
    public static T GetService<T>() where T : class
    {
        return Services?.GetRequiredService<T>() ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found");
    }
}
