using Kk.Kharts.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kk.Kharts.Api.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;

            // Aplica o middleware apenas nas rotas que começam com /Api/ApiKey
            if (path.StartsWithSegments("/Api/ApiKey"))
            {
                // Exibe todos os headers recebidos
                foreach (var header in context.Request.Headers)
                {
                    _logger.LogDebug("ApiKey header: {Key}={Value}", header.Key, header.Value);
                }

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Obtém todos os usuários que têm chave de API configurada
                var users = await dbContext.Users
                    .Where(u => u.HeaderName != null && u.HeaderValue != null)
                    .ToListAsync();

                bool autorizado = false;

                foreach (var user in users)
                {
                    if (context.Request.Headers.TryGetValue(user.HeaderName!, out var valorCabecalho) &&
                        valorCabecalho == user.HeaderValue)
                    {
                        autorizado = true;
                        break;
                    }
                }

                if (!autorizado)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Accès non autorisé : clé API invalide.");
                    return;
                }
            }

            // Continua a execução do pipeline
            await _next(context);
        }
    }
}