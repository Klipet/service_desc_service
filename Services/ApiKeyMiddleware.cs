using DevExpress.Xpo;
using Microsoft.AspNetCore.Authorization;
using System;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly UnitOfWork _uow;

    public ApiKeyMiddleware(RequestDelegate next, UnitOfWork uow)
    {
        _next = next;
        _uow = uow;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;

        if (path.StartsWithSegments("/ticketHub") ||
            path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        var isPublic = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
        if (isPublic)
        {
            await _next(context);
            return;
        }

        string? apiKey = null;
        if (context.Request.Headers.TryGetValue("X-API-KEY", out var headerKey))
        {
            apiKey = headerKey.ToString();
        }
        else if (context.Request.Query.TryGetValue("access_token", out var queryKey))
        {
            apiKey = queryKey.ToString();
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API key missing" });
            return;
        }

        try
        {
            // ✅ Используйте инжектированный UoW вместо GetNewUnitOfWork()
            var user = _uow.Query<User>()
                .FirstOrDefault(u => u.ApiKey == apiKey);

            if (user == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
                return;
            }

            if (user.RoleUser == null || !user.RoleUser.IsActive)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { error = "User role is inactive" });
                return;
            }

            context.Items["CurrentUser"] = user;
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
            return;
        }

        await _next(context);
    }
}
