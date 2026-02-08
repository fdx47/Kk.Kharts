using Kk.Kharts.Api.Utility.Constants;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Kk.Kharts.Api.Attributes
{
    public class DenyAccessForRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly bool _isWriteAccessRequired;

        // Construtor que recebe um parâmetro booleano
        // Se for true, exige acesso de leitura e escrita (WriteAccessRoles)
        // Se for false, exige apenas acesso de leitura (ReadOnlyAccessRoles)
        public DenyAccessForRoleAttribute(bool isWriteAccessRequired = true)
        {
            _isWriteAccessRequired = isWriteAccessRequired;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {           
                var userRole = context.HttpContext.User?.FindFirst(ClaimTypes.Role)?.Value;

            // Verifica se o role do usuário é válido (presente na lista de roles)
            if (!IsValidRole(userRole!))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();  // Retorna "Forbidden"
                return;
            }

            // Verifica se o usuário tem o tipo de acesso necessário (escrita ou somente leitura)
            if (_isWriteAccessRequired)
            {
                // Se for necessário acesso de escrita, verifica se o usuário tem o role de escrita
                if (!Roles.WriteAccessRoles.Contains(userRole))
                {
                    context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();  // Negar acesso se não tiver role de escrita
                }
            }
            else
            {
                // Se for necessário apenas leitura, verifica se o usuário tem o role de leitura
                if (!Roles.ReadOnlyAccessRoles.Contains(userRole))
                {
                    context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();  // Negar acesso se não tiver role de leitura
                }
            }
        }

        // Método para verificar se o role é válido (presente na classe Roles)
        private bool IsValidRole(string role)
        {
            return Roles.WriteAccessRoles.Contains(role) || Roles.ReadOnlyAccessRoles.Contains(role);
        }
    }
}
