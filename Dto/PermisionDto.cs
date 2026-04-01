public class PermisionDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime DateCreated { get; set; }
    public RolePermission RolePermissions { get; set; }
}

public class AssignRoleDto
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}