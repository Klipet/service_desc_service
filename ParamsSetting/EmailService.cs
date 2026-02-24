using MailKit.Net.Pop3;
using MailKit.Security;
using MimeKit;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
        Console.WriteLine("EmailService создан");
    }

    public List<MimeMessage> GetEmails()
    {
        var messages = new List<MimeMessage>();
        try
        {
            using var client = new Pop3Client();
            // Проверяем что конфиг читается
            var host = _config["Email:Host"];
                var port = _config["Email:Port"];
                var user = _config["Email:Username"];
                var pass = _config["Email:Password"];
                // Подключение к серверу
                client.Connect(host, int.Parse(port), SecureSocketOptions.Auto);

            // Аутентификация
            client.Authenticate(user, pass);

            //    client.Authenticate("user@mail.com", "password");

            Console.WriteLine($"Всего писем на сервере: {client.Count}");


                // Получение всех писем
                for (int i = 0; i < client.Count; i++)
                {
                    var message = client.GetMessage(i);
                    messages.Add(message);
                    client.DeleteMessage(i);
                }
            
                // Отключение
                client.Disconnect(true);
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении писем: {ex.Message}");

        }
        return messages;
    }
}