using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string HEADER = "X-API-KEY";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var uow = context.HttpContext.RequestServices.GetRequiredService<UnitOfWork>();
       
        if (!context.HttpContext.Request.Headers.TryGetValue(HEADER, out var apiKeyHeader))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        string apiKey = apiKeyHeader.FirstOrDefault();

        var user = uow.Query<User>().FirstOrDefault(x => x.ApiKey == apiKey);

        if (user == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        context.HttpContext.Items["CurrentUser"] = user;

        await next();
    }
}
