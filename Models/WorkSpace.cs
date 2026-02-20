using DevExpress.Xpo;

[Persistent("WorkSpace")]
public class WorkSpace: BaseEntity
{
    public WorkSpace(Session session) : base(session){}
    [Association("WorkSpace-User")]
    public XPCollection<User> Users => GetCollection<User>(nameof(Users));

    [Association("WorkSpace-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

  
}
