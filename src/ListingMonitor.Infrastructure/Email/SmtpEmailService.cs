using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ListingMonitor.Infrastructure.Email;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task<bool> TestConnectionAsync();
}

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public SmtpEmailService(SmtpSettings settings)
    {
        _settings = settings;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        
        try
        {
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, 
                _settings.UseSsl ? SecureSocketOptions.SslOnConnect : 
                _settings.UseStartTls ? SecureSocketOptions.StartTls : 
                SecureSocketOptions.None);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        using var client = new SmtpClient();
        
        try
        {
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, 
                _settings.UseSsl ? SecureSocketOptions.SslOnConnect : 
                _settings.UseStartTls ? SecureSocketOptions.StartTls : 
                SecureSocketOptions.None);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class SmtpSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; }
    public bool UseStartTls { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
}
