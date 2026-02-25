using DevExpress.Xpo;
using MimeKit;

public class TiketFromEmail
{
 
    public Tiket CreateTiketFromEmail( MimeMessage email)
    {
        using var uow = MyXPO.GetNewUnitOfWork();
        Author? autor = null;
        Company? company = null;
        Platform platform = null;
        Tiket? existingTicket = null;

        string? emailAddress = email.From.Mailboxes.FirstOrDefault()?.Address;

        autor = uow.Query<Author>().FirstOrDefault(a => a.Email == emailAddress);
        if (autor == null)
        {
            autor = uow.Query<Author>().FirstOrDefault(a => a.Oid == 1);
        }
        platform = uow.Query<Platform>()
                  .FirstOrDefault(p => p.Authors.Any(a => a.Oid == autor.Oid));

        company = uow.Query<Company>()
                  .FirstOrDefault(p => p.Platforms.Any(a => a.Oid == platform.Oid));


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
            DataCreted = DateTime.UtcNow,
            ResaultPhone = false,
            BugTransfer = false,
            DueDate = DateTime.Now.AddHours(2),
        };


        // Сохраняем тикет в базе
        uow.Save(tiket);
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