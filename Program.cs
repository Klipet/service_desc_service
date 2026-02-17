
using DevExpress.Xpo;
using DevExpress.Xpo.DB;


    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1️⃣ СНАЧАЛА инициализация подключения
            var core = new Core();
            core.InitializeConnection();

            // 2️⃣ Теперь можно обновлять базу
            MyXPO.UpdateDataBase();

            // 3️⃣ Регистрируем UnitOfWork
            builder.Services.AddScoped(provider =>
            {
                return MyXPO.GetNewUnitOfWork();
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }

