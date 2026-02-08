//using Kk.Kharts.Api.Data;
//using Kk.Kharts.Shared.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace Kk.Kharts.Api.Services
//{
//    public interface IUserAuthenticationService
//    {
//        Task<Company?> AuthenticateFromHeaders(IHeaderDictionary headers);
//    }

//    public class UserAuthenticationService : IUserAuthenticationService
//    {
//        private readonly AppDbContext _context;

//        public UserAuthenticationService(AppDbContext context)
//        {
//            _context = context;
//        }

//        /// <summary>
//        /// Méthode privée pour authentifier un utilisateur en parcourant les en-têtes HTTP
//        /// </summary>
//        //public async Task<User?> AuthenticateFromHeaders(IHeaderDictionary headers)
//        //{
//        //    //_logger.LogInformation("Tentative d'authentification via les en-têtes...");

//        //    foreach (var header in headers)
//        //    {
//        //        string headerKey = header.Key;
//        //        string headerValue = header.Value.ToString();

//        //        if (string.IsNullOrWhiteSpace(headerValue)) continue;

//        //        var user = await _context.Companies
//        //            .FirstOrDefaultAsync(u => u.HeaderNameApiKey == headerKey && u.HeaderValueApiKey == headerValue);

//        //        if (user != null)
//        //        {
//        //            //_logger.LogInformation("Utilisateur trouvé pour l'en-tête: {Key}", headerKey);
//        //            return user;
//        //        }
//        //    }

//        //    return null;
//        //}
//        public async Task<Company?> AuthenticateFromHeaders(IHeaderDictionary headers)
//        {
//            //_logger.LogInformation("Tentative d'authentification via les en-têtes...");

//            foreach (var header in headers)
//            {
//                string headerKey = header.Key;
//                string headerValue = header.Value.ToString();

//                if (string.IsNullOrWhiteSpace(headerValue)) continue;

//                var company = await _context.Companies
//                    .FirstOrDefaultAsync(u => u.HeaderNameApiKey == headerKey && u.HeaderValueApiKey == headerValue);

//                if (company != null)
//                {
//                    //_logger.LogInformation("Societe trouvé pour l'en-tête: {Key}", headerKey);
//                    return company;
//                }
//            }

//            return null;
//        }
//    }

//}
