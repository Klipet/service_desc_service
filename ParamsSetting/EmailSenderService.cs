using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailSenderService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailSenderService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    // Старая реализация на System.Net.Mail.SmtpClient — оставлена как
    // справка. НЕ РАБОТАЕТ для кастомного Message-ID/References/In-Reply-To:
    // System.Net.Mail.SmtpClient игнорирует Message-ID, поставленный через
    // MailMessage.Headers, и генерирует свой при отправке — поэтому якорь
    // треда никогда не долетал до реального письма (в EmailQueue поля
    // заполнялись верно, но в заголовках отправленного письма их не было).
    //
    // public async Task SendEmailAsync(
    // List<string> toList,
    // string subject,
    // string body,
    // bool isHtml = true,
    // string messageId = null,
    // string threadAnchorId = null)
    // {
    //     using var client = new System.Net.Mail.SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
    //     {
    //         Credentials = new System.Net.NetworkCredential(
    //             _smtpSettings.UserName,
    //             _smtpSettings.Password),
    //         EnableSsl = _smtpSettings.EnableSsl
    //     };
    //
    //     var message = new System.Net.Mail.MailMessage
    //     {
    //         From = new System.Net.Mail.MailAddress(_smtpSettings.From),
    //         Subject = subject,
    //         Body = body,
    //         IsBodyHtml = isHtml
    //     };
    //
    //     foreach (var to in toList)
    //         message.To.Add(to);
    //
    //     if (!string.IsNullOrEmpty(messageId))
    //         message.Headers.Add("Message-ID", messageId);
    //
    //     if (!string.IsNullOrEmpty(threadAnchorId))
    //     {
    //         message.Headers.Add("References", threadAnchorId);
    //         message.Headers.Add("In-Reply-To", threadAnchorId);
    //     }
    //
    //     await client.SendMailAsync(message);
    // }

    public async Task SendEmailAsync(
    List<string> toList,
    string subject,
    string body,
    bool isHtml = true,
    string messageId = null,
    string threadAnchorId = null)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_smtpSettings.From));
        foreach (var to in toList)
            message.To.Add(MailboxAddress.Parse(to));

        message.Subject = subject;
        message.Body = new TextPart(isHtml ? "html" : "plain") { Text = body };

        // MimeKit сам оборачивает id в угловые скобки — обрезаем свои,
        // если GenerateMessageId их уже добавил, чтобы не задвоить "<<..>>".
        if (!string.IsNullOrEmpty(messageId))
            message.MessageId = messageId.Trim('<', '>');

        if (!string.IsNullOrEmpty(threadAnchorId))
        {
            var anchor = threadAnchorId.Trim('<', '>');
            message.InReplyTo = anchor;
            message.References.Add(anchor);
        }

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _smtpSettings.Host,
            _smtpSettings.Port,
            _smtpSettings.EnableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None);
        await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}