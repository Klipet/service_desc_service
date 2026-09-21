public class PermisionDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime DateCreated { get; set; }
    public List<RolePermissionDto> RolePermissions { get; set; }
}

public class AssignRoleDto
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}

public class RolePermissionDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; }
}