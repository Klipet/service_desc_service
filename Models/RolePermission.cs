using DevExpress.Xpo;
[Persistent("RolePermission")]
public class RolePermission: XPObject
{
    public RolePermission(Session session) : base(session) { }

    [Association("Role-Permissions")]
    public Role Role { get; set; }

    [Association("Permission-RolePermissions")]
    public Permission Permission { get; set; }
}
