using DevExpress.Schedule;
using DevExpress.Xpo;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Xml.Linq;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

public class EmptyDataDB
{
    public static void SendData(UnitOfWork uow)
    {
        if (!uow.Query<State>().Any())
        {
            new State(uow) { Name = "Открыт" };
            new State(uow) { Name = "Закрыт" };
            new State(uow) { Name = "В процессе" };
            new State(uow) { Name = "Новая заявка" };
            
        }
        if (!uow.Query<Preority>().Any())
        {
            new Preority(uow) { Name = "Низкий" };
            new Preority(uow) { Name = "Средний" };
            new Preority(uow) { Name = "Высокий" };
            
        }
        if (!uow.Query<WorkSpace>().Any())
        {
            new WorkSpace(uow) { Name = "Тех-отдел" };
            
        }
        if (!uow.Query<TiketType>().Any())
        {
            new TiketType(uow) { Name = "E-mail" };
            new TiketType(uow) { Name = "Нормальный" };
            new TiketType(uow) { Name = "Нормальный" };

        }

        if (!uow.Query<SubCategory>().Any())
        {
            new SubCategory(uow) { Name = "Нормальный" };
           
        }
        if (!uow.Query<Category>().Any())
        {
            new Category(uow) { Name = "Нормальный" };
           
        }

        if (!uow.Query<Author>().Any())
        {
            new Author(uow) { Name = "Нормальный" };
            
        }
        if (!uow.Query<User>().Any())
        {
            new User(uow) { Name = "Admin", Loghin = "Admin", PasswordHash= BCrypt.Net.BCrypt.HashPassword("Admin"), };
            
        }

        if (!uow.Query<Platform>().Any())
        {
            new Platform(uow) { Name = "Нормальный" };
          
        }

        if (!uow.Query<CompanyState>().Any())
        {
            new CompanyState(uow) { Name = "Нормальный"};
            new CompanyState(uow) { Name = "VIP"};
           
        }
        if (!uow.Query<Company>().Any())
        {
            new Company(uow) { Name = "Нормальный"};
          
        }

        if (!uow.Query<Mode>().Any())
        {
            new Mode(uow) { Name = "Нормальный" };
        
        }

        if (!uow.Query<HolidayDay>().Any())
        {
            var holidays = new List<(string Name, int Day, int Month)>
    {
        ("Новый год", 1, 1),
        ("Рождество (православное)", 7, 1),
        ("Международный женский день", 8, 3),
        ("День труда", 1, 5),
        ("День победы", 9, 5),
        ("День детей", 1, 6),
        ("День независимости Молдовы", 27, 8),
        ("Limba noastră", 31, 8),
        ("Рождество (католическое)", 25, 12),
    };

            foreach (var h in holidays)
            {
                new HolidayDay(uow)
                {
                    Name = h.Name,
                    Date = new DateTime(2000, h.Month, h.Day),
                    IsRecurringYearly = true
                };
            }
           

        }
        if (!uow.Query<Role>().Any())
        {
            var adminRole = new Role(uow) { Name = "Admin", IsActive = true };
            var supportRole = new Role(uow) { Name = "Support", IsActive = true };
            var userRole = new Role(uow) { Name = "User", IsActive = true };
          
        }

        if (!uow.Query<Permission>().Any())
        {

            var adminPermission = uow.Query<Role>().FirstOrDefault(p => p.Oid == 1);
            var permissions = new[]
            {
                // Заявки
                PermisionConstant.TicketRead,
                PermisionConstant.TicketCreate,
                PermisionConstant.TicketUpdate,
                PermisionConstant.TicketDelete,
                PermisionConstant.TicketAssign,
                PermisionConstant.TicketAttach,
                 // Пользователи
                PermisionConstant.UserRead,
                PermisionConstant.UserCreate,
                PermisionConstant.UserUpdate,
                PermisionConstant.UserDelete,
                // Ответы
                PermisionConstant.CommentRead,
                PermisionConstant.CommentCreate,
                PermisionConstant.CommentDelete,
                //полномочия
                PermisionConstant.PermisionCreate,
                PermisionConstant.PermisionUpdate,
                PermisionConstant.PermisionDelete,
                PermisionConstant.PermisionRead,
                // роли
                PermisionConstant.RoleUpdate,
                PermisionConstant.RoleDelete,
                PermisionConstant.RoleRead,
                PermisionConstant.RoleCreate,

                PermisionConstant.AuthorCreate,
                PermisionConstant.AuthorUpdate,
                PermisionConstant.AuthorDelete,
                PermisionConstant.AuthorRead,
                PermisionConstant.CategoryUpdate,
                PermisionConstant.CategoryDelete,
                PermisionConstant.CategoryRead,
                PermisionConstant.CategoryCreate,
                PermisionConstant.CompanyRead,
                PermisionConstant.CompanyUpdate,
                PermisionConstant.CompanyDelete,
                PermisionConstant.CompanyCreate,
                PermisionConstant.EmailTemplateRead,
                PermisionConstant.EmailTemplateUpdate,
                PermisionConstant.EmailTemplateDelete,
                PermisionConstant.EmailTemplateCreate,
                PermisionConstant.HolidayRead,
                PermisionConstant.HolidayUpdate,
                PermisionConstant.HolidayDelete,
                PermisionConstant.HolidayCreate,
                PermisionConstant.ModeRead,
                PermisionConstant.ModeUpdate,
                PermisionConstant.ModeDelete,
                PermisionConstant.ModeCreate,
                PermisionConstant.PlatformRead,
                PermisionConstant.PlatformUpdate,
                PermisionConstant.PlatformDelete,
                PermisionConstant.PlatformCreate,
                PermisionConstant.PreorityRead,
                PermisionConstant.PreorityUpdate,
                PermisionConstant.PreorityDelete,
                PermisionConstant.PreorityCreate,
                PermisionConstant.StateRead,
                PermisionConstant.StateUpdate,
                PermisionConstant.StateDelete,
                PermisionConstant.StateCreate,
                PermisionConstant.SubCategoryRead,
                PermisionConstant.SubCategoryUpdate,
                PermisionConstant.SubCategoryDelete,
                PermisionConstant.SubCategoryCreate,
                PermisionConstant.TiketTypeRead,
                PermisionConstant.TiketTypeCreate,
                PermisionConstant.TiketTypeUpdate,
                PermisionConstant.TiketTypeDelete,
                PermisionConstant.WorkScheduleRead,
                PermisionConstant.WorkScheduleUpdate,
                PermisionConstant.WorkScheduleDelete,
                PermisionConstant.WorkScheduleCreate,
                PermisionConstant.WorkSpaceRead,
                PermisionConstant.WorkSpaceUpdate,
                PermisionConstant.WorkSpaceDelete,
                PermisionConstant.WorkSpaceCreate
            };

            foreach (var name in permissions)
            {
                var permission = new Permission(uow)
                {
                    Name = name,
                    IsActive = true,
                    DateCreated = DateTime.Now
                };

                new RolePermission(uow)
                {
                    Role = adminPermission,
                    Permission = permission
                };
            }

        }
        uow.CommitChanges();
    }
}
