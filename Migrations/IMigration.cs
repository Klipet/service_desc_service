using DevExpress.Xpo;

public interface IMigration
{
    int Version { get; }
    string Description { get; }
    void Up(UnitOfWork uow);
}
