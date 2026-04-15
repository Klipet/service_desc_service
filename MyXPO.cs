
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using System.Data;

public class MyXPO
{
    private static readonly object _lockObject = new object();
    private static IDataLayer _dataLayer = null;
    private static IDataStore _dataStore = null;

    public static IDataStore DataStore
    {
        get
        {
            return _dataStore;
        }
    }
    public static DevExpress.Xpo.Session GetNewSession()
    {
        return new DevExpress.Xpo.Session(GetDataLayer());
    }
    // Method to get a new UnitOfWork
    public static DevExpress.Xpo.UnitOfWork GetNewUnitOfWork()
    {
        DevExpress.Xpo.XpoDefault.Session = null;
        return new DevExpress.Xpo.UnitOfWork(GetDataLayer());
    }

    // Method to get DB DateTime
    public static DateTime GetDBDateTime(Session session)
    {
        return (DateTime)session.Evaluate(typeof(DevExpress.Xpo.XPObjectType), CriteriaOperator.Parse("Now()"), null);
    }
    // Method to reset the connection
    public static void Reset()
    {
        lock (_lockObject)
        {
            if (_dataStore != null)
            {
                CloseDataStore(_dataStore);
                _dataStore = null;
            }
            try
            {
                if (_dataLayer != null)
                {
                    _dataLayer.Dispose();
                    _dataLayer = null;
                }
            }
            catch (Exception ex)
            {
                //LogManager.WriteException(ex);
            }
        }
    }

    // Connection string property
    public static string ConnectionString { get; set; }

    // Method to get the data layer
    public static IDataLayer GetDataLayer()
    {
        lock (_lockObject)
        {
            if (_dataStore == null)
            {
                _dataStore = CreateDataStore(ConnectionString, AutoCreateOption.SchemaAlreadyExists);
            }

            if (_dataLayer == null)
            {
                _dataLayer = CreateDataLayer(_dataStore, AutoCreateOption.SchemaAlreadyExists);
            }
        }

        return (IDataLayer)_dataLayer;
    }

    // Method to create a data store
    private static IDataStore CreateDataStore(string connectionString, AutoCreateOption autoCreate)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        return DevExpress.Xpo.XpoDefault.GetConnectionProvider(connectionString, autoCreate);
    }

    // Method to close a data store
    private static void CloseDataStore(IDataStore dataStore, string v = "")
    {
        try
        {
            if (dataStore is DataStorePool poolStore)
            {
                poolStore.Dispose();
            }
            else
            {
                var conM = dataStore.GetType().GetProperty("Connection");
                if (conM != null)
                {
                    var con = (IDbConnection)conM.GetGetMethod().Invoke(dataStore, null);
                    con?.Close();
                    con?.Dispose();
                }
            }

            //LogManager.WriteEntry("DataStore " + v + " connection closed", TraceEventType.Information);
        }
        catch (Exception ex)
        {
            // LogManager.WriteEntry("CloseDataStore Exception:" + ex.ToString(), TraceEventType.Error);
        }
    }

    // Method to create a data layer from a data store
    private static IDataLayer CreateDataLayer(IDataStore dataStore, AutoCreateOption autoCreateOption, XPDictionary dict = null)
    {
        DevExpress.Xpo.XpoDefault.Session = null;
        DevExpress.Xpo.XpoDefault.DataLayer = null;

        if (autoCreateOption == AutoCreateOption.DatabaseAndSchema)
        {
            return XpoDefault.GetDataLayer(ConnectionString, AutoCreateOption.DatabaseAndSchema);
        }
        else
        {
            if (dict == null)
            {
                using (var session = new Session())
                {
                    session.ConnectionString = ConnectionString;
                    session.AutoCreateOption = autoCreateOption;
                    session.Connect();
                    dict = session.Dictionary;
                }

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                dict.GetDataStoreSchema(assemblies);
            }

            return new DevExpress.Xpo.ThreadSafeDataLayer(dict, dataStore);
        }
    }

    // Method to update the database
    public static void UpdateDataBase()
    {
        var dict = new ReflectionDictionary();

        // регистрируем все классы XPO
        dict.GetDataStoreSchema(typeof(Program).Assembly);

        var dataStore = CreateDataStore(
            ConnectionString?.Replace("XpoDataStorePool=True;", ""),
            AutoCreateOption.DatabaseAndSchema
        );

        var dataLayer = new SimpleDataLayer(dict, dataStore);

        using (var session = new Session(dataLayer))
        {
            session.UpdateSchema(dict.CollectClassInfos(typeof(Program).Assembly));
        }

        dataLayer.Dispose();
        CloseDataStore(dataStore, "UpdateDataBase");
    }
}

