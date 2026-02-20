public class Core
{
    private static bool UpdatedDatabase = false;
    private readonly IConfiguration _configuration;

    // Initialize the XPO connection
    public Core(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public void InitializeConnection()
    {
        /*       string connectionString =
            "XpoProvider=Postgres;" +
            "Server=localhost;" +
            "Port=5432;" +
            "User ID=postgres;" +
            "Password=Admin@123;" +
            "Database=servicedesck;" +
            "XpoDataStorePool=True;";
               Connect(connectionString);
       */
        var connectionString = _configuration.GetConnectionString("XpoConnection")
            ?? throw new InvalidOperationException("Connection string 'XpoConnection' not found.");

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

         //   using (var uow = MyXPO.GetNewSession())
         //   {
         //       var obj = uow.GetObjectByKey<Country>(1);
        //    }

            retObj = true;
        }
        catch (Exception ex)
        {
            MyXPO.Reset();
            // GlobalCore.LogManager.WriteEntry(ex.ToString(), TraceEventType.Error);
        }

        return retObj;
    }
}

