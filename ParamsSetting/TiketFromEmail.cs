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

        var tiket = new Tiket(uow)
        {
            Title = email.Subject ?? "(Без темы)",
            Description = email.TextBody ?? email.HtmlBody ?? "",
            Company = company,
            Platform = platform,
            Author = autor,
            DataCreted = DateTime.UtcNow,
            ResaultPhone = false,
            BugTransfer = false,
            DueDate = DateTime.UtcNow.AddHours(2),
        };


        // Сохраняем тикет в базе
        uow.Save(tiket);
        uow.CommitChanges();

        return tiket;
    }
}