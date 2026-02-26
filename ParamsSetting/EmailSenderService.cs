using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public class EmailSenderService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailSenderService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(List<string> toList, string subject, string body)
    {
        using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
            EnableSsl = _smtpSettings.EnableSsl
        };

        var message = new MailMessage
        {
            From = new MailAddress(_smtpSettings.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        foreach (var to in toList)
        {
            message.To.Add(to);
        }
        await client.SendMailAsync(message);
        
    }
}