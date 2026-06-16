public class MigrationRunner
{
    private readonly List<IMigration> _migrations;

    public MigrationRunner()
    {
        _migrations = typeof(MigrationRunner).Assembly.GetTypes()
         .Where(t => typeof(IMigration).IsAssignableFrom(t)
                  && !t.IsInterface
                  && !t.IsAbstract)
         .Select(t => (IMigration)Activator.CreateInstance(t)!)
         .OrderBy(m => m.Version)
         .ToList();
    }

    public void Run()
    {
        EnsureMigrationsTable();

        int currentVersion = GetCurrentVersion();
        Console.WriteLine($"[Migration] Current version: {currentVersion}");

        var pending = _migrations
            .Where(m => m.Version > currentVersion)
            .OrderBy(m => m.Version)
            .ToList();

        if (!pending.Any())
        {
            Console.WriteLine("[Migration] Already up to date.");
            return;
        }

        foreach (var migration in pending)
        {
            Console.WriteLine($"[Migration] Applying v{migration.Version}: {migration.Description}");
            using var uow = MyXPO.GetNewUnitOfWork();
            try
            {
                migration.Up(uow);
                uow.CommitChanges();
                SetVersion(migration.Version, migration.Description);
                Console.WriteLine($"[Migration] v{migration.Version} OK.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Migration] FAILED v{migration.Version}: {ex.Message}");
                throw;
            }
        }
    }

    private static void EnsureMigrationsTable()
    {
        using var session = MyXPO.GetNewSession();
        session.ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS db_migrations (
                version     INTEGER PRIMARY KEY,
                description TEXT NOT NULL,
                applied_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
        ");
    }

    private static int GetCurrentVersion()
    {
        using var session = MyXPO.GetNewSession();
        var result = session.ExecuteScalar(
            "SELECT COALESCE(MAX(version), 0) FROM db_migrations;"
        );
        return Convert.ToInt32(result);
    }

    private static void SetVersion(int version, string description)
    {
        using var session = MyXPO.GetNewSession();
        session.ExecuteNonQuery(
            $"INSERT INTO db_migrations (version, description) VALUES ({version}, '{description}') ON CONFLICT (version) DO NOTHING;"
        );
    }
}
