using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListingMonitor.Domain.Entities;
using ListingMonitor.Infrastructure.Data;
using ListingMonitor.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

namespace ListingMonitor.UI.ViewModels;

public partial class SmtpSettingsViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly Action _onClose;
    
    [ObservableProperty] private string _smtpHost = "smtp.gmail.com";
    [ObservableProperty] private int _smtpPort = 587;
    [ObservableProperty] private bool _useStartTls = true;
    [ObservableProperty] private string _smtpUsername = "";
    [ObservableProperty] private string _smtpPassword = "";
    [ObservableProperty] private string _fromEmail = "";
    [ObservableProperty] private string _fromName = "İlan Takip";
    [ObservableProperty] private int _checkIntervalMinutes = 10;
    
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private bool _isTestInProgress;

    public SmtpSettingsViewModel(AppDbContext context, Action onClose)
    {
        _context = context;
        _onClose = onClose;
        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _context.AppSettings.ToListAsync();
            
            if (settings.Any())
            {
                var dict = settings.ToDictionary(s => s.Key, s => s.Value);
                
                if (dict.TryGetValue("SmtpHost", out var host)) SmtpHost = host;
                if (dict.TryGetValue("SmtpPort", out var port) && int.TryParse(port, out var p)) SmtpPort = p;
                if (dict.TryGetValue("UseStartTls", out var tls) && bool.TryParse(tls, out var t)) UseStartTls = t;
                if (dict.TryGetValue("SmtpUsername", out var user)) SmtpUsername = user;
                if (dict.TryGetValue("SmtpPassword", out var pass)) SmtpPassword = pass;
                if (dict.TryGetValue("FromEmail", out var from)) FromEmail = from;
                if (dict.TryGetValue("FromName", out var name)) FromName = name;
                if (dict.TryGetValue("CheckIntervalMinutes", out var interval) && int.TryParse(interval, out var i)) CheckIntervalMinutes = i;
            }
        }
        catch (Exception ex)
        {
            Message = $"Ayarlar yüklenemedi: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SmtpHost) || string.IsNullOrWhiteSpace(FromEmail))
            {
                Message = "SMTP Host ve Gönderici Email zorunludur.";
                return;
            }

            // Helper to save/update setting
            async Task SaveSetting(string key, string value)
            {
                var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
                if (setting == null)
                {
                    setting = new AppSetting { Key = key, Value = value };
                    _context.AppSettings.Add(setting);
                }
                else
                {
                    setting.Value = value;
                }
            }

            await SaveSetting("SmtpHost", SmtpHost);
            await SaveSetting("SmtpPort", SmtpPort.ToString());
            await SaveSetting("UseStartTls", UseStartTls.ToString());
            await SaveSetting("SmtpUsername", SmtpUsername);
            await SaveSetting("SmtpPassword", SmtpPassword);
            await SaveSetting("FromEmail", FromEmail);
            await SaveSetting("FromName", FromName);
            await SaveSetting("CheckIntervalMinutes", CheckIntervalMinutes.ToString());

            await _context.SaveChangesAsync();

            // Update runtime settings
            var smtpSettings = ServiceLocator.GetService<SmtpSettings>();
            if (smtpSettings != null)
            {
                smtpSettings.SmtpHost = SmtpHost;
                smtpSettings.SmtpPort = SmtpPort;
                smtpSettings.UseStartTls = UseStartTls;
                smtpSettings.Username = SmtpUsername;
                smtpSettings.Password = SmtpPassword;
                smtpSettings.FromEmail = FromEmail;
                smtpSettings.FromName = FromName;
                Console.WriteLine($"📧 SMTP ayarları güncellendi:");
                Console.WriteLine($"   🌐 Host: {smtpSettings.SmtpHost}");
                Console.WriteLine($"   👤 Username: {smtpSettings.Username}");
                Console.WriteLine($"   📤 FromEmail: {smtpSettings.FromEmail}");
            }
            else
            {
                Console.WriteLine("❌ SmtpSettings singleton bulunamadı!");
            }

            Message = "SMTP ayarları başarıyla kaydedildi!";
            
            await Task.Delay(2000);
            _onClose?.Invoke();
        }
        catch (Exception ex)
        {
            Message = $"Kaydetme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task TestEmail()
    {
        if (string.IsNullOrWhiteSpace(SmtpHost) || string.IsNullOrWhiteSpace(FromEmail))
        {
            Message = "Lütfen önce SMTP Host ve Gönderici Email giriniz.";
            return;
        }

        try
        {
            IsTestInProgress = true;
            Message = "Test maili gönderiliyor...";
            
            // Create temp settings for testing
            var testSettings = new SmtpSettings
            {
                SmtpHost = SmtpHost,
                SmtpPort = SmtpPort,
                UseStartTls = UseStartTls,
                Username = SmtpUsername,
                Password = SmtpPassword,
                FromEmail = FromEmail,
                FromName = FromName
            };

            var emailService = new SmtpEmailService(testSettings);
            
            var subject = $"İlan Takip SMTP Test - {DateTime.Now:dd.MM.yyyy HH:mm}";
            var body = $@"<html><body>
                <h2>✅ SMTP Test Başarılı</h2>
                <p>Bu email, İlan Takip uygulamasının SMTP ayarlarının doğru yapılandırıldığını doğrulamaktadır.</p>
                <hr>
                <p><strong>SMTP Host:</strong> {SmtpHost}</p>
                <p><strong>SMTP Port:</strong> {SmtpPort}</p>
                <p><strong>Use StartTLS:</strong> {UseStartTls}</p>
                <p><strong>From Email:</strong> {FromEmail}</p>
                <p><strong>Test Tarihi:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>
                <hr>
                <p><em>Bu otomatik bir test mailidir. Lütfen yanıtlamayınız.</em></p>
                </body></html>";

            await emailService.SendEmailAsync(FromEmail, subject, body);
            
            Console.WriteLine($"✅ SMTP Test maili gönderildi: {FromEmail}");
            
            Message = $"✅ Test maili başarıyla gönderildi! ({FromEmail})";
        }
        catch (Exception ex)
        {
            Message = $"❌ Test mail hatası: {ex.Message}";
            Console.WriteLine($"❌ SMTP Test hatası: {ex.Message}");
        }
        finally
        {
            IsTestInProgress = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onClose?.Invoke();
    }

    [RelayCommand]
    private void LoadGmailDefaults()
    {
        SmtpHost = "smtp.gmail.com";
        SmtpPort = 587;
        UseStartTls = true;
        Message = "Gmail varsayılan ayarları yüklendi. App Password kullanmayı unutmayın!";
    }

    [RelayCommand]
    private void LoadOutlookDefaults()
    {
        SmtpHost = "smtp-mail.outlook.com";
        SmtpPort = 587;
        UseStartTls = true;
        Message = "Outlook varsayılan ayarları yüklendi.";
    }

    [RelayCommand]
    private void LoadYahooDefaults()
    {
        SmtpHost = "smtp.mail.yahoo.com";
        SmtpPort = 587;
        UseStartTls = true;
        Message = "Yahoo varsayılan ayarları yüklendi. App Password kullanmayı unutmayın!";
    }
}
