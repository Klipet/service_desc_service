using Microsoft.AspNetCore.SignalR;
using static DevExpress.Xpo.Helpers.AssociatedCollectionCriteriaHelper;

public class EmailBackgroundService : BackgroundService
{
    private readonly ILogger<EmailBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);
    private readonly IHubContext<TicketHub> _hub;

    public EmailBackgroundService(
        ILogger<EmailBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IHubContext<TicketHub> hub
        )
    {
        _hub = hub;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                var tiketFromEmail = scope.ServiceProvider.GetRequiredService<TiketFromEmail>();


                var emails = emailService.GetEmails();
                _logger.LogInformation($"Получено писем: {emails.Count}");

                foreach (var email in emails)
                {
                    try
                    {
                        var tiсket = tiketFromEmail.CreateTiketFromEmail(email);


                        if (tiсket.IsNewTicket)
                        {
                            _logger.LogInformation($"отправляю Ticket : {tiсket.Ticket.Id}");
                            await _hub.Clients.All.SendAsync("NewTicketCreated", tiсket.Ticket);

                        }
                        else
                        {
                            _logger.LogInformation($"отправляю Comment : {tiсket.Comment.Id}");
                            await _hub.Clients.All.SendAsync("NewComment", tiсket.Comment);
                        }
                       
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при обработке письма: {Subject}", email.Subject);
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при обработке письма: {ex}");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}