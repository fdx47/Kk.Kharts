
//using Kk.Kharts.Api.Utility.Constants;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc.Filters;
//using System;

//namespace Kk.Kharts.Api.Attributes
//{

//public class NoAuthorizeAttribute : Attribute, IAuthorizationFilter
//{
//    private readonly string _role;

//    public NoAuthorizeAttribute(string role)
//    {
//        _role = role;
//    }

//    public void OnAuthorization(AuthorizationFilterContext context)
//    {
//        var userRole = context.HttpContext.User?.FindFirst("role")?.Value;

//        if (userRole == _role)
//        {
//            context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult(); // Retorna "Forbidden"
//        }
//    }
//}



//    public class NoAuthorizeAttribute : Attribute, IAuthorizationFilter
//    {
//        private readonly string _role;

//        public NoAuthorizeAttribute(string role)
//        {
//            _role = role;

//            // Verifica se o role passado é válido
//            if (!IsValidRole(_role))
//            {
//                // Se não for um role válido, já negamos o acesso
//                throw new UnauthorizedAccessException($"Role '{role}' is not a valid role.");
//            }
//        }

//        public void OnAuthorization(AuthorizationFilterContext context)
//        {
//            var userRole = context.HttpContext.User?.FindFirst("role")?.Value;

//            // Se o role do usuário não for válido, nega o acesso
//            if (!IsValidRole(userRole!))
//            {
//                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();  // Negar o acesso imediatamente
//            }

//            // Se o role do usuário for igual ao do atributo, também negamos o acesso
//            if (userRole == _role)
//            {
//                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();  // Negar o acesso
//            }
//        }

//        // Método para verificar se o role é válido (presente na classe Roles)
//        private bool IsValidRole(string role)
//        {
//            return role == Roles.Root ||
//                   role == Roles.SuperAdmin ||
//                   role == Roles.Admin ||
//                   role == Roles.UserRW ||
//                   role == Roles.User ||
//                   role == Roles.Technician ||
//                   role == Roles.Demo;
//        }
//    }
//}
