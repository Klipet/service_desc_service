using DevExpress.Xpo;
[NonPersistent]
public abstract class BaseEntity : XPObject
{
    public BaseEntity(Session session) : base(session) { }

    private string _name;
    private bool _active;
    private DateTime _dateCreated;
    private DateTime _dateModified;

    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    public bool Active
    {
        get => _active;
        set => SetPropertyValue(nameof(Active), ref _active, value);
    }

    public DateTime DateCreated
    {
        get => _dateCreated;
        set => SetPropertyValue(nameof(DateCreated), ref _dateCreated, value);
    }

    public DateTime DateModified
    {
        get => _dateModified;
        set => SetPropertyValue(nameof(DateModified), ref _dateModified, value);
    }
}

