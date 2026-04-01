using DevExpress.Xpo;
[Persistent("Role")]
public class Role : XPObject
{
    public Role(Session session) : base(session) { }

    private string _name;
    private bool _isActive;
    private DateTime _dateCreated;

    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }
    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
    }
    public DateTime DateCreated
    {
        get => _dateCreated;
        set => SetPropertyValue(nameof(DateCreated), ref _dateCreated, value);
    }

    [Association("Role-Users")]
    public XPCollection<User> Users => GetCollection<User>(nameof(Users));

    [Association("Role-Permissions")]
    public XPCollection<RolePermission> RolePermissions => GetCollection<RolePermission>(nameof(RolePermissions));

}

