public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime DateCreated { get; set; }
    public User Users { get; set; }
    public RolePermission RolePermissions { get; set; }

}

