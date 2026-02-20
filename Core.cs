public class Core
{
    private static bool UpdatedDatabase = false;
    private readonly IConfiguration _configuration;

    public Core(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void InitializeConnection()
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection не задана");

        Connect(connectionString);
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
            retObj = true;
        }
        catch (Exception ex)
        {
            MyXPO.Reset();
        }
        return retObj;
    }
}
