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

    public async Task SendEmailAsync(
    List<string> toList,
    string subject,
    string body,
    bool isHtml = true,
    string messageId = null,
    string threadAnchorId = null)
    {
        using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            Credentials = new NetworkCredential(
                _smtpSettings.UserName,
                _smtpSettings.Password),
            EnableSsl = _smtpSettings.EnableSsl
        };

        var message = new MailMessage
        {
            From = new MailAddress(_smtpSettings.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        foreach (var to in toList)
            message.To.Add(to);

        // Свой Message-ID (и якорь треда в References/In-Reply-To) — иначе
        // id генерит SMTP-сервер сам, и его нельзя сопоставить обратно
        // с Tiket.MessageId, когда клиент отвечает на письмо.
        if (!string.IsNullOrEmpty(messageId))
            message.Headers.Add("Message-ID", messageId);

        if (!string.IsNullOrEmpty(threadAnchorId))
        {
            message.Headers.Add("References", threadAnchorId);
            message.Headers.Add("In-Reply-To", threadAnchorId);
        }

        await client.SendMailAsync(message);
    }
}