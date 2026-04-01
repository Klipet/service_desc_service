using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class PermissionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var requirePermission = context.ActionDescriptor.EndpointMetadata
            .OfType<RequirePermissionAttribute>()
            .FirstOrDefault();

        // Нет атрибута — доступ открыт
        if (requirePermission == null) return;

        var user = context.HttpContext.Items["CurrentUser"] as User;

        // Проверяем есть ли нужное право у роли пользователя
        var hasPermission = user?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == requirePermission.Permission
                    && rp.Permission.IsActive);

        if (hasPermission != true)
        {
            context.Result = new ObjectResult(new
            {
                error = "Access denied",
                required = requirePermission.Permission,
                userRole = user?.RoleUser?.Name
            })
            { StatusCode = 403 };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}