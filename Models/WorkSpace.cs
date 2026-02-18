using DevExpress.Xpo;

[Persistent("WorkSpace")]
public class WorkSpace: XPObject
{
    public WorkSpace(Session session) : base(session){}
    private string _name;
    private bool _active;
    private DateTime _dateCreated;
    private DateTime _dateModified;

    [Association("WorkSpace-User")]
    public XPCollection<User> Users => GetCollection<User>(nameof(Users));

    [Association("WorkSpace-Tikets")]
    public XPCollection<NewTiket> Tikets => GetCollection<NewTiket>(nameof(Tikets));
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), value);
    }
    public bool Active
    {
        get => _active;
        set => SetPropertyValue(nameof(Active), value);
    }
    public DateTime DateCreated
    {
        get => _dateCreated;
        set => SetPropertyValue(nameof(_dateCreated), value);
    }
    public DateTime DateModified
    {
        get => _dateModified;
        set => SetPropertyValue(nameof(DateModified), value);
    }
}
