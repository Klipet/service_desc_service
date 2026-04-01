using DevExpress.Xpo;
[Persistent("Permission")]
public class Permission: XPObject
{
    public Permission(Session session) : base(session) { }

    private string _name;
    private bool _isActive;
    private bool _code;
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

    [Association("Permission-RolePermissions")]
    public XPCollection<RolePermission> RolePermissions => GetCollection<RolePermission>(nameof(RolePermissions));
}