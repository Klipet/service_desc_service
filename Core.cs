public class Core
{
    private static bool UpdatedDatabase = false;
    private readonly IConfiguration _configuration;

    public Core(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool InitializeConnection()
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection не задана");

       return Connect(connectionString);
    }

    private static bool Connect(string connectionString = "")
    {
        bool retObj = false;
        try
        {
            MyXPO.ConnectionString = connectionString;
            if (!UpdatedDatabase)
            {
                MyXPO.UpdateDataBase();
                UpdatedDatabase = true;
            }
            Console.WriteLine("[OK] Successfully connected to the database.");
            retObj = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to connect to the database: {ex.Message}");
            MyXPO.Reset();
        }
        return retObj;
    }
}
