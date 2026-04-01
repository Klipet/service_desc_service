using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiKey]
[Route("[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public PermissionsController(UnitOfWork uow)
    { _uow = uow; }

    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    [HttpGet("GetAll[controller]")]
    public IActionResult GetAll()
    {
        if (!HasPermission(PermisionConstant.PermisionRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var permisions = _uow.Query<Permission>().ToList();

            var resault = permisions.Select(p => new PermisionDto
            {
                Id = p.Oid,
                Name = p.Name,
                IsActive = p.IsActive,
                DateCreated = p.DateCreated,
            }).ToList();

            return Ok(resault);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }
    // Назначить роль пользователю
    [HttpPut("AssignRole")]
    public IActionResult AssignRole([FromBody] AssignRoleDto dto)
    {
        if (!HasPermission(PermisionConstant.PermisionUpdate))
            return StatusCode(403, new { error = "Access denied" });

        var user = _uow.Query<User>()
            .FirstOrDefault(u => u.Oid == dto.UserId);

        if (user == null)
            return NotFound(new { error = "User not found" });

        var role = _uow.Query<Role>()
            .FirstOrDefault(r => r.Oid == dto.RoleId);

        if (role == null)
            return NotFound(new { error = "Role not found" });

        user.RoleUser = role;
        _uow.CommitChanges();

        return Ok(new { message = $"Role '{role.Name}' assigned to '{user.Name}'" });
    }

    // Назначить права роли
    [HttpPut("PermissionsToRole")]
    public IActionResult SetPermissions([FromQuery] int roleId, [FromBody] List<int> permissionIds)
    {
        if (!HasPermission(PermisionConstant.RoleRead))
            return StatusCode(403, new { error = "Access denied" });

        var role = _uow.Query<Role>()
            .FirstOrDefault(r => r.Oid == roleId);

        if (role == null)
            return NotFound(new { error = "Role not found" });

        // Удаляем старые права
        var oldPermissions = _uow.Query<RolePermission>()
            .Where(rp => rp.Role.Oid == roleId)
            .ToList();
        foreach (var old in oldPermissions)
            _uow.Delete(old);

        // Назначаем новые
        foreach (var permId in permissionIds)
        {
            var permission = _uow.Query<Permission>()
                .FirstOrDefault(p => p.Oid == permId);

            if (permission != null)
            {
                new RolePermission(_uow)
                {
                    Role = role,
                    Permission = permission
                };
            }
        }

        _uow.CommitChanges();
        return Ok(new { message = "Permissions updated" });
    }
}