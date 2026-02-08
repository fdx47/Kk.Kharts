using Kk.Kharts.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public ApiKeyMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;

            // Aplica o middleware apenas nas rotas que começam com /Api/ApiKey
            if (path.StartsWithSegments("/Api/ApiKey"))
            {
                // Exibe todos os headers recebidos
                Console.WriteLine("Headers recebidos:");
                foreach (var header in context.Request.Headers)
                {
                    Console.WriteLine($" {header.Key}: {header.Value}");
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





////Resumo funcional:

////Quando o cliente acessa uma rota que começa com /Api/ApiKey:

////O middleware exibe todos os headers recebidos no console.

////Acessa o banco de dados (AppDbContext) para buscar um usuário com chave de API registrada (campos HeaderName e HeaderValue).

////Se não existir usuário com chave configurada, responde com 403 Forbidden.

////Se o header esperado não estiver presente ou tiver valor incorreto, responde com 401 Unauthorized.

////Caso tudo esteja válido, a requisição continua normalmente.