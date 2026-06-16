using DevExpress.Xpo;

public class Migration002: IMigration
    {
        public int Version => 2;
        public string Description => "Add user";

        public void Up(UnitOfWork uow)
    {
        if (!uow.Query<User>().Any(p => p.Name == "User"))
        {
            new User(uow) { Name = "User", Loghin = "User", PasswordHash = BCrypt.Net.BCrypt.HashPassword("User"),};

        }
    }

    }

