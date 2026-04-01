using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Microsoft.OpenApi.Models;

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

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:8080", "http://localhost:5000") // ← порты твоего Flutter Web
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // ← обязательно для SignalR
            });
        });
  
        builder.Services.AddScoped<IReportService,    ReportService>();
        builder.Services.AddScoped<IReportRepository, ReportRepository>();

        builder.Services.AddSingleton<EmailService>();
        builder.Services.AddSingleton<EmailSenderService>();
        builder.Services.AddHostedService<EmailBackgroundService>();
        builder.Services.AddHostedService<EmailSenderBackgroundService>();
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

        builder.Services.AddSignalR();

        builder.Services.AddTransient<TiketFromEmail>(); // ← эта строка отсутствует
        builder.Services.AddSingleton<GenerateJwtToken>();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<PermissionFilter>(); // ← добавить
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });


            builder.Services.AddSwaggerGen();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key needed to access the endpoints. X-API-KEY: {key}",
                In = ParameterLocation.Header,
                Name = "X-API-KEY",
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
        });
        builder.Services.AddSingleton<S3Service>();
     //   builder.WebHost.UseUrls("http://0.0.0.0:5000");
        var app = builder.Build();

        app.UseCors();
        app.MapHub<TicketHub>("/ticketHub");

        using var uow = MyXPO.GetNewUnitOfWork();
        EmptyDataDB.SendData(uow);
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseMiddleware<ApiKeyMiddleware>();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
