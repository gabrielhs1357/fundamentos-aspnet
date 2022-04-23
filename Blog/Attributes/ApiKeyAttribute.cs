using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Blog.Attributes
{
    // Indica que pode ser usado em métodos e classes
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Query.TryGetValue(
                Configuration.ApiKeyName, out var ApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "ApiKey não encontrada"
                };

                return;
            }

            if (!Configuration.APIKey.Equals(ApiKey))
            {
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "Acesso não autorizado"
                };

                return;
            }

            await next();
        }
    }
}
