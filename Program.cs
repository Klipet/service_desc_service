using DevExpress.Xpo;
using DevExpress.Xpo.DB;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var core = new Core(builder.Configuration);
        if (!core.InitializeConnection())
        {
            Console.WriteLine("[FATAL] Cannot start application: database connection failed.");
            return;
        }
        core.InitializeConnection();

        builder.Services.AddScoped(provider => MyXPO.GetNewUnitOfWork());
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

        builder.Services.AddSingleton<EmailService>();
        builder.Services.AddHostedService<EmailBackgroundService>();
        builder.Services.AddTransient<TiketFromEmail>(); // ← эта строка отсутствует
        builder.Services.AddSingleton<GenerateJwtToken>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<S3Service>();
    //    builder.WebHost.UseUrls("http://0.0.0.0:5000");
        var app = builder.Build();
        using var uow = MyXPO.GetNewUnitOfWork();
        EmptyDataDB.SendData(uow);
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
