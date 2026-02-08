using Kk.Kharts.Api.Services.Ingestion;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kk.Kharts.Api.Attributes
{
    public class ApiKeyAuthorizeKkAttribute : Attribute, IAsyncActionFilter
    {       
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resolver = context.HttpContext.RequestServices.GetRequiredService<IApiKeyResolver>();
            var company = await resolver.ResolveAsync(context.HttpContext.Request.Headers);

            if (company == null)
            {
                context.Result = new UnauthorizedObjectResult("Accès non autorisé.");
                return;
            }

            context.HttpContext.Items["Company"] = company;

            await next();
        }
    }
}
