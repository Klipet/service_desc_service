using DevExpress.Xpo;

public class Migration003: IMigration
{
    public int Version => 3;
    public string Description => "Add pg_trgm index on Company.Name";

    public void Up(UnitOfWork uow)
    {
        uow.ExecuteNonQuery("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        uow.ExecuteNonQuery(@"
            CREATE INDEX IF NOT EXISTS idx_company_name_trgm 
            ON ""Company"" USING gin (""Name"" gin_trgm_ops);
        ");
    }

}

