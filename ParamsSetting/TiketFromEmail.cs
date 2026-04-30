using DevExpress.Xpo;
using MimeKit;
using System.Net.Mail;
using static DevExpress.Xpo.Helpers.AssociatedCollectionCriteriaHelper;

public class TiketFromEmail
{
    private readonly ILogger<TiketFromEmail> _logger;

    public TiketFromEmail(ILogger<TiketFromEmail> logger)
    {
        _logger = logger;
    }


    public TiketEmailServiceDto CreateTiketFromEmail(MimeMessage email)
    {
        using var uow = MyXPO.GetNewUnitOfWork();
        Author? autor = null;
        Company? company = null;
        Platform platform = null;
        Tiket? existingTicket = null;
        Mode? existMode = null;

        string? emailAddress = email.From.Mailboxes.FirstOrDefault()?.Address;
        if (emailAddress != null || emailAddress != "")
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
            var comment = new TiketCommentDto
            {
                Id = message.Oid,
                Tiket = existingTicket.Oid,
                Author = autor.Oid,
                MailMessageId = email.MessageId,
                CreatedAt = message.CreatedAt,
                MessageText = message.MessageText ?? ""
            };


            return new TiketEmailServiceDto
            {
                IsNewTicket = false,
                Comment = comment
            }; ;
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

        return new TiketEmailServiceDto
        {
            IsNewTicket = true,
            Ticket = new TiketResponseDto
            {
                Id = tiket.Oid,
                Title = tiket.Title ?? string.Empty,
                Description = tiket.Description ?? string.Empty,

                AuthorId = tiket.Author != null ? tiket.Author.Oid : 0,
                AuthorName = tiket.Author != null ? tiket.Author.Name : string.Empty,

                CategoryName = tiket.Category != null ? tiket.Category.Name : string.Empty,

                Phone = tiket.Phone ?? string.Empty,

                CompanyId = tiket.Company != null ? tiket.Company.Oid : 0,
                CompanyName = tiket.Company != null ? tiket.Company.Name : string.Empty,

                SubCategoryName = tiket.SubCategory != null ? tiket.SubCategory.Name : string.Empty,

                StateName = tiket.State != null ? tiket.State.Name : string.Empty,
                StateId = tiket.State != null ? tiket.State.Oid : 0,

                TypeTiketName = tiket.TypeTiket != null ? tiket.TypeTiket.Name : string.Empty,

                PlatformId = tiket.Platform != null ? tiket.Platform.Oid : 0,
                PlatformName = tiket.Platform != null ? tiket.Platform.Name : string.Empty,

                WorkSpaceName = tiket.WorkSpace != null ? tiket.WorkSpace.Name : string.Empty,

                UserId = tiket.User != null ? tiket.User.Oid : 0,
                UserName = tiket.User != null ? tiket.User.Name : string.Empty,

                PreorityName = tiket.Preorety != null ? tiket.Preorety.Name : string.Empty,

                DataPhone = tiket.DataPhone,
                ResaultPhone = tiket.ResaultPhone,
                DateSecondPhone = tiket.DateSecondPhone,
                BugNumber = tiket.BugNumber ?? string.Empty,
                BugTransfer = tiket.BugTransfer,
                ModeName = tiket.Mode != null ? tiket.Mode.Name : string.Empty,
                DataCreted = tiket.DataCreted,
                DataModefire = tiket.DataModefire,
                DueDate = tiket.DueDate ?? DateTime.UtcNow,

                // 📎 Файлы тикета
                Files = tiket.Files.Select(f => new TiketFileDto
                {
                    Id = f.Oid,
                    FileName = f.FileName,
                    FileUrl = f.FileUrl,
                    IsResponse = f.IsResponse
                }).ToList(),

                // 💬 Решения
                Solution = tiket.Solutions.Select(s => new TiketSolutionDto
                {
                    Id = s.Oid,
                    Author = s.Author != null ? s.Author.Oid : 0,
                    User = s.User != null ? s.User.Oid : 0,
                    MessageText = s.MessageText ?? string.Empty,
                    CreatedAt = s.CreatedAt,

                    // если EmailList хранится строкой через ;
                    EmailList = string.IsNullOrEmpty(s.EmailList)
                        ? new List<string> { s.Author.Email }
                        : s.EmailListParsed
                }).ToList(),

                Comment = tiket.Messages.Select(m => new TiketCommentDto
                {
                    Id = m.Oid,
                    Tiket = m.Tiket.Oid,
                    Author = m.Author != null ? m.Author.Oid : 0,
                    MailMessageId = m.EmailMessageId,
                    CreatedAt = m.CreatedAt,
                    MessageText = m.MessageText ?? string.Empty,
                }).ToList(),
            }
        };
    
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