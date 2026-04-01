using DevExpress.Xpo;

public class EmailQueueService
{
    private readonly UnitOfWork _uow;
    private readonly EmailTemplateService _templateService;

    public EmailQueueService(UnitOfWork uow)
    {
        _uow = uow;
        _templateService = new EmailTemplateService(uow);
    }

    public void EnqueueTicketAnswer(
    Tiket tiket,
    User user,
    string messageText,
    List<string> emailListRaw)
    {
    //    var settings = _uow.Query<CompanySettings>()
     //       .FirstOrDefault();

        var variables = TicketTemplateVariables.Build(
            tiket,
            user,
            messageText
        //    settings
            );

        var emailData = _templateService.Render(
         //   EmailEventType.TicketAnswered,
            tiket,
            variables);

        var recipients = emailListRaw;

        if (!recipients.Any())
        {
            recipients = new List<string> { tiket.Author?.Email };
        }



        foreach (var email in recipients)
        {
            new EmailQueue(_uow)
            {
                To = email,
                Subject = emailData.subject,
                Body = emailData.body,
                IsHtml = emailData.isHtml,
                IsSent = false,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            };
        }

        _uow.CommitChanges();
    }
}
