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

        var variables = TicketTemplateVariables.Build(
            tiket,
            user,
            messageText
            );

        var emailData = _templateService.Render(
            tiket,
            variables);

        var recipients = emailListRaw;

        if (!recipients.Any())
        {
            recipients = new List<string> { tiket.Author?.Email };
        }

        // Якорь треда: первое письмо по тикету становится Tiket.MessageId.
        // Все следующие письма несут этот якорь в References/In-Reply-To —
        // почтовые клиенты сами тащат его дальше в цепочке ответов, поэтому
        // TiketFromEmail.CreateTiketFromEmail находит тикет по References
        // независимо от того, на какое письмо треда ответил клиент.
        bool isFirstEmailForTicket = string.IsNullOrEmpty(tiket.MessageId);

        foreach (var email in recipients)
        {
            var ownMessageId = GenerateMessageId(tiket.Oid);

            // Якорем становится Message-ID самого первого отправленного
            // письма по тикету (первый получатель первого вызова).
            if (string.IsNullOrEmpty(tiket.MessageId))
            {
                tiket.MessageId = ownMessageId;
            }
            var threadAnchorId = isFirstEmailForTicket && email == recipients.First()
                ? null
                : tiket.MessageId;

            new EmailQueue(_uow)
            {
                To = email,
                Subject = emailData.subject,
                Body = emailData.body,
                IsHtml = emailData.isHtml,
                IsSent = false,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0,
                MessageIdHeader = ownMessageId,
                ThreadAnchorId = threadAnchorId
            };
        }

        _uow.CommitChanges();
    }

    private static string GenerateMessageId(int ticketOid)
    {
        return $"<ticket-{ticketOid}-{Guid.NewGuid():N}@service-desk.internal>";
    }
}
