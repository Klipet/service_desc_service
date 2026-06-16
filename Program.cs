using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();


            var core = new Core(builder.Configuration);
            if (args.Contains("--migrate"))
            {
                var connectionString = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .Build()
                    .GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection не задана");

                MyXPO.ConnectionString = connectionString;
                MyXPO.UpdateDataBase();

                var runner = new MigrationRunner();
                runner.Run();

                Console.WriteLine("[Migrator] Done.");
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
                    policy.WithOrigins("http://localhost:8080", "http://localhost:5000", "https://dev-servicedesk.edi.md") // ← порты твоего Flutter Web
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // ← обязательно для SignalR
                });
            });

            builder.Services.AddScoped<IReportService, ReportService>();
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
            builder.WebHost.UseUrls("http://0.0.0.0:5000");
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                options.Limits.MinRequestBodyDataRate = null;
            });



            var app = builder.Build();

            app.UseRouting();
            app.UseCors();
            app.UseWebSockets();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseMiddleware<ApiKeyMiddleware>();
            app.UseAuthorization();

            app.MapHub<TicketHub>("/ticketHub");
            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DI Error: {ex.InnerException?.Message ?? ex.Message}");
            throw;
        }
        finally
        {
            LogManager.Shutdown();
        }
    }
}
