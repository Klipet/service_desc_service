using DevExpress.Xpo;

public class Migration004_UpdateDependencies : IMigration
{
    public int Version => 5; // поставьте следующий свободный номер
    public string Description => "Обновить зависимости: переименовать колонку и восстановить FK";

    public void Up(UnitOfWork uow)
    {
        // 1) Добавить новую колонку в SubCategory, если её ещё нет.
        //    Имя колонки по умолчанию = имени свойства ("Category"), а не "CategoryOid",
        //    т.к. в модели явно не задано другое имя персистентности.
        uow.ExecuteNonQuery(@"
                ALTER TABLE ""SubCategory""
                ADD COLUMN IF NOT EXISTS ""Category"" integer;
            ");

        // 2) Перенести данные из старой связи (Category.SubCategory) в новую (SubCategory.Category).
        //    Если несколько Category ссылались на одну и ту же SubCategory,
        //    берём одну из них (здесь — с минимальным Oid) во избежание неоднозначности.
        //    !!! Проверьте эту логику под свои бизнес-требования — возможна потеря данных.
        uow.ExecuteNonQuery(@"
                UPDATE ""SubCategory"" AS s
                SET ""Category"" = sub.category_oid
                FROM (
                    SELECT ""SubCategory"" AS subcategory_oid, MIN(""OID"") AS category_oid
                    FROM ""Category""
                    WHERE ""SubCategory"" IS NOT NULL
                    GROUP BY ""SubCategory""
                ) AS sub
                WHERE s.""OID"" = sub.subcategory_oid
                  AND s.""Category"" IS NULL;
            ");

        // 3) Добавить FK constraint SubCategory.Category -> Category.Oid
        uow.ExecuteNonQuery(@"
                ALTER TABLE ""SubCategory""
                DROP CONSTRAINT IF EXISTS subcategory_category_fkey;

                ALTER TABLE ""SubCategory""
                ADD CONSTRAINT subcategory_category_fkey
                FOREIGN KEY (""Category"") REFERENCES ""Category""(""OID"") ON DELETE SET NULL;
            ");

        // 4) Старую колонку "SubCategory" в таблице "Category" пока лучше НЕ удалять сразу —
        //    оставьте на одну итерацию как страховку, потом почистите отдельной миграцией:
        //
        // uow.ExecuteNonQuery(@"
        //     ALTER TABLE ""Category"" DROP COLUMN IF EXISTS ""SubCategory"";
        // ");

        // Сначала снимаем FK constraint на старую колонку, если он существует
        uow.ExecuteNonQuery(@"
                ALTER TABLE ""Category""
                DROP CONSTRAINT IF EXISTS category_subcategory_fkey;
            ");

        // Удаляем саму колонку
        uow.ExecuteNonQuery(@"
                ALTER TABLE ""Category""
                DROP COLUMN IF EXISTS ""SubCategory"";
            ");


        uow.CommitChanges();
    }
}

