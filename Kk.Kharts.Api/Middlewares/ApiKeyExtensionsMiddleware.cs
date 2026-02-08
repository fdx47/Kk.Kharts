namespace Kk.Kharts.Api.Middlewares
{
    public static class ApiKeyExtensionsMiddleware
    {
        public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }
    }
}
