using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using StravaRunner.Core.Models.Settings;

namespace StravaRunner.Core.Services;

public interface IEmailService
{
    Task SendEmailAsync(string from, string recipient, string subject, string body,
        CancellationToken cancellationToken = default);

    Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken = default);
}

public class EmailService(IOptions<SmtpSettings> smtpSettings) : IEmailService
{
    private readonly SmtpSettings _smtpSettings = smtpSettings.Value;

    public async Task SendEmailAsync(string from, string recipient, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        using var smtpClient = GetSmtpClient();
        
        await smtpClient.SendMailAsync(from, recipient, subject, body, cancellationToken);
    }
    
    public async Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken = default)
    {
        using var smtpClient = GetSmtpClient();
        
        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }

    private SmtpClient GetSmtpClient()
    {
        var smtpClient = new SmtpClient(_smtpSettings.SmtpServer, _smtpSettings.Port);
        smtpClient.Credentials = new NetworkCredential(_smtpSettings.Credentials.Username, _smtpSettings.Credentials.Password);
        smtpClient.EnableSsl = _smtpSettings.EnableSsl;
        
        return smtpClient;
    }
}