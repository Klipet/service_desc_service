using DevExpress.Data.Filtering;
using DevExpress.Xpo;

public class EmailSenderBackgroundService: BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailSenderBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public EmailSenderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<EmailSenderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var uow = MyXPO.GetNewUnitOfWork();

                var sender = scope.ServiceProvider
                    .GetRequiredService<EmailSenderService>();

                var emails = new XPCollection<EmailQueue>(
                    uow,
                    CriteriaOperator.Parse("IsSent = false AND RetryCount < 5")
                );

                foreach (var email in emails)
                {
                    try
                    {
                        await sender.SendEmailAsync(
                            new List<string> { email.To },
                            email.Subject,
                            email.Body,
                            email.IsHtml
                        );

                        email.IsSent = true;
                        email.SentAt = DateTime.UtcNow;
                        email.ErrorMessage = null;
                    }
                    catch (Exception ex)
                    {
                        email.RetryCount++;
                        email.LastAttemptAt = DateTime.Now;
                        email.ErrorMessage = ex.Message;

                        _logger.LogError(ex,
                            $"Ошибка отправки письма на {email.To}");
                    }
                }

                uow.CommitChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка EmailSenderBackgroundService");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}