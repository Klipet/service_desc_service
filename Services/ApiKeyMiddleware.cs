using DevExpress.Xpo;
using Microsoft.AspNetCore.Authorization;
using System;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? apiKey = null;
        // Публичные эндпоинты пропускаем
        var endpoint = context.GetEndpoint();
        var isPublic = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
        if (isPublic)
        {
            await _next(context);
            return;
        }

        // Читаем ApiKey из заголовка
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
        // Ищем пользователя по ApiKey
        using var uow = MyXPO.GetNewUnitOfWork();
        var user = uow.Query<User>()
            .FirstOrDefault(u => u.ApiKey == apiKey.ToString());

        if (user == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        // Проверяем что роль активна
        if (user.RoleUser == null || !user.RoleUser.IsActive)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "User role is inactive" });
            return;
        }

        // Кладём пользователя в контекст
        context.Items["CurrentUser"] = user;

        await _next(context);
    }
}
