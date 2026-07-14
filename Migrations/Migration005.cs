using DevExpress.Xpo;

public class Migration006 : IMigration
{
    public int Version => 6; // укажите следующий свободный номер после последней миграции
    public string Description => "Backfill Author/Phone dates and seed test data";


    public void Up(UnitOfWork uow)
    {
        // 1) Гарантированно создаём/обновляем схему именно для нужных классов,
        //    не полагаясь на внешний MyXPO.UpdateDataBase()
        uow.UpdateSchema(typeof(Author), typeof(Phone));
        uow.ExecuteNonQuery(@"
            INSERT INTO ""Phone"" (""Number"", ""Author"", ""DataCreated"", ""DataModefire"")
            SELECT TRIM(""Phone""), ""OID"", NOW(), NOW()
            FROM ""Autor""
            WHERE ""Phone"" IS NOT NULL AND TRIM(""Phone"") <> '';
        ");
    }
}

