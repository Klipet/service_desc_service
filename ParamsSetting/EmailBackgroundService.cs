using Microsoft.AspNetCore.SignalR;

public class EmailBackgroundService: BackgroundService
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

                        await _hub.Clients.All.SendAsync("NewTicketCreated", new
                        {
                            Id = tiсket.Oid,
                            Title = tiсket.Title,
                            Description = tiсket.Description,
                            AuthorName = tiсket.Author?.Name ?? "",
                            AuthorId = tiсket.Author?.Oid ?? 0,
                            CategoryName = "",
                            Phone = "",
                            CompanyName = tiсket.Company.Name,
                            CompanyId = tiсket.Company.Oid,
                            SubCategoryName = "",
                            StateName = "",
                            TypeTiketName = "",
                            PlatformName = tiсket.Platform.Name,
                            PlatformId = tiсket.Platform.Oid,
                            WorkSpaceName = "",
                            UserName = "",
                            UserId = 0,
                            PreorityName = "",
                            DataPhone = DateTime.Now,
                            ResaultPhone = false,
                            DateSecondPhone = DateTime.Now,
                            BugNumber = "",
                            BugTransfer = false,
                            ModeId = 0,
                            ModeName = "",
                            DataCreted = DateTime.Now,
                            DueDate = tiсket.DueDate ?? DateTime.UtcNow,
                        });
                    }catch(Exception ex)
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