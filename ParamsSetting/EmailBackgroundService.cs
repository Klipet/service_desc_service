public class EmailBackgroundService: BackgroundService
{
    private readonly ILogger<EmailBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public EmailBackgroundService(
        ILogger<EmailBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
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
                  
                    var tiket = tiketFromEmail.CreateTiketFromEmail(email);
                    
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении писем");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}