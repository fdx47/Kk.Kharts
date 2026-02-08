using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kk.Kharts.Api.Middlewares;

/// <summary>
/// Filtro que adiciona requisitos de segurança às operações do Swagger
/// baseado nos atributos [Authorize], [JwtAuth] e [ApiKeyAuth].
/// </summary>
public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var declaringType = context.MethodInfo.DeclaringType;
        if (declaringType is null) return;

        var methodAttributes = context.MethodInfo.GetCustomAttributes(true);
        var classAttributes = declaringType.GetCustomAttributes(true);
        var allAttributes = classAttributes.Concat(methodAttributes);

        var hasApiKey = allAttributes.OfType<ApiKeyAuthAttribute>().Any();
        var hasJwt = allAttributes.OfType<JwtAuthAttribute>().Any();
        var hasAuthorize = allAttributes.OfType<AuthorizeAttribute>().Any();

        operation.Security ??= [];

        if (hasApiKey)
        {
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("ApiKey"),
                    new List<string>()
                }
            });
        }

        if (hasJwt || hasAuthorize)
        {
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                }
            });
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class JwtAuthAttribute : Attribute;
