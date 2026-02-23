using DevExpress.Xpo;
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
        }

        if (!uow.Query<TiketType>().Any())
        {
            new TiketType(uow) { Name = "Нормальный" };
        }

        if (!uow.Query<TiketType>().Any())
        {
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

        if (!uow.Query<Platform>().Any())
        {
            new Platform(uow) { Name = "Нормальный" };
        }

        if (!uow.Query<Company>().Any())
        {
            new Company(uow) { Name = "Нормальный" };
        }

        if (!uow.Query<Mode>().Any())
        {
            new Mode(uow) { Name = "Нормальный" };
        }

        uow.CommitChanges();
    }
}
