using DevExpress.Xpo;
using MimeKit;
using System.Net.Mail;

public class TiketFromEmail
{
    private readonly ILogger<TiketFromEmail> _logger;

    public TiketFromEmail(ILogger<TiketFromEmail> logger)
    {
        _logger = logger;
    }


    public Tiket CreateTiketFromEmail( MimeMessage email)
    {
        using var uow = MyXPO.GetNewUnitOfWork();
        Author? autor = null;
        Company? company = null;
        Platform platform = null;
        Tiket? existingTicket = null;
        Mode? existMode = null;

        string? emailAddress = email.From.Mailboxes.FirstOrDefault()?.Address;
        if(emailAddress != null || emailAddress != "")
        {
            existMode = uow.Query<Mode>().FirstOrDefault(m => m.Oid == 1);
           
        }

        autor = uow.Query<Author>().FirstOrDefault(a => a.Email == emailAddress);
        if (autor == null)
        {
            autor = uow.Query<Author>().FirstOrDefault(a => a.Oid == 1);
        }

        // Если автор всё ещё null — дальше идти нельзя
        if (autor == null)
        {
            _logger.LogWarning("Автор не найден для письма: {Email}", emailAddress);
            return null;
        }

        platform = uow.Query<Platform>()
            .FirstOrDefault(p => p.Authors.Any(a => a.Oid == autor.Oid));

        if (platform == null)
        {
            // Берём дефолтную платформу
            platform = uow.Query<Platform>().FirstOrDefault(p => p.Oid == 1);
        }

        if (platform == null)
        {
            _logger.LogWarning("Платформа не найдена для автора: {AutorId}", autor.Oid);
            return null;
        }

        company = uow.Query<Company>()
            .FirstOrDefault(p => p.Platforms.Any(a => a.Oid == platform.Oid));

        if (company == null)
        {
            // Берём дефолтную компанию
            company = uow.Query<Company>().FirstOrDefault(c => c.Oid == 1);
        }


        if (!string.IsNullOrEmpty(email.InReplyTo))
        {
            
            
            existingTicket = uow.Query<TiketComment>().FirstOrDefault(m => m.EmailMessageId == email.InReplyTo)?.Tiket;
            Console.WriteLine($"Проверка InReplyTo : {existingTicket}");

            if (existingTicket == null && email.References != null)
            {
                foreach (var reference in email.References)
                {
                    var message = uow.Query<Tiket>()
                        .FirstOrDefault(m => m.MessageId == reference);

                    if (message != null)
                    {
                   
                        existingTicket = message;
                        Console.WriteLine($"Проверка reference: {existingTicket}");
                        break;
                    }
                }
            }

        }
        
        if (existingTicket != null)
        {
            var rawText = email.TextBody ?? email.HtmlBody ?? "";
            var cleanText = ExtractNewMessage(rawText);


            var message = new TiketComment(uow)
            {
                Tiket = existingTicket,
                MessageText = cleanText,
            //    email.TextBody ?? email.HtmlBody ?? "",
                CreatedAt = DateTime.Now,
                Author = autor,
                EmailMessageId = email.MessageId
            };
           
            uow.Save(message);
            uow.CommitChanges();
            Console.WriteLine($"Создан Comment: {message.Oid}");
            return existingTicket;
        }
        var tiket = new Tiket(uow)
        {
            MessageId = email.MessageId,
            Email = emailAddress ?? "",
            Title = email.Subject ?? "(Без темы)",
            Description = email.TextBody ?? email.HtmlBody ?? "",
            Company = company,
            Platform = platform,
            Author = autor,
            DataCreted = DateTime.Now,
            ResaultPhone = false,
            BugTransfer = false,
            Mode = existMode,
            DueDate = DateTime.Now.AddHours(2),
        };

      



        // Сохраняем тикет в базе
        uow.Save(tiket);

        if (email.Attachments != null && email.Attachments.Any())
        {
            Console.WriteLine("В письме есть вложения");
            foreach (var attachment in email.Attachments)
            {
                if (attachment is MimePart part)
                {
                    using var stream = new MemoryStream();
                    part.Content.DecodeTo(stream);

                    var fileBytes = stream.ToArray();

                    string base64String = Convert.ToBase64String(fileBytes);

                    Console.WriteLine($"Файл: {part.FileName}");
                    Console.WriteLine($"Base64 длина: {base64String.Length}");

                    // Здесь можешь сохранить в БД

                    var tiketfile = new TiketFile(uow)
                    {
                        FileName = attachment.ContentDisposition.FileName.ToString(),
                        FileUrl = base64String,
                        IsResponse = false,
                        Tiket = tiket,
                        TiketSolution = null
                    };
                    uow.Save(tiketfile);
                }
            }

        }

        

        uow.CommitChanges();

        return tiket;
    }

    public string ExtractNewMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return body;

        var lines = body.Split('\n');
        var result = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Если началась цитата — останавливаемся
            if (trimmed.StartsWith(">") ||
                trimmed.Contains("в ") && trimmed.Contains("г. в") ||
                trimmed.StartsWith("On "))
            {
                break;
            }

            result.Add(line);
        }

        return string.Join("\n", result).Trim();
    }
}