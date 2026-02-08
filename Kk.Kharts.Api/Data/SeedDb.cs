using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Data
{
    public class SeedDb(AppDbContext context)
    {
        private readonly AppDbContext _context = context;

        public class Seed()
        {
            // Método para inicializar dados do banco de dados, se necessário
            public static void SeedData(WebApplication app)
            {
                using var scope = app.Services.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<SeedDb>();
                service.SeedAsync().Wait();
            }

        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            // Verifica se já existe alguma societe para evitar duplicação
            if (!await _context.Companies.AnyAsync())
            {
                _context.Companies.AddRange(
                    new Company { Id = 1, Name = "3CTEC" },
                    new Company { Id = 2, Name = "Stratberries" },
                    new Company { Id = 3, Name = "Invenio" },
                    new Company { Id = 4, Name = "Pozzobon" },
                    new Company { Id = 5, Name = "Baudas" }
                    //new Societe { Id = 6, Nom = "3CTEC Biz 1", SocieteMereId = 1 },   // Filial de 3ctec
                    //new Societe { Id = 7, Nom = "3CTEC Biz 2", SocieteMereId = 1 },   // Filial de 3ctec
                );
                await _context.SaveChangesAsync();
            }

            await CheckUsersAsync(nom: "Mister César",      email: "cesar@blazor.com",            password: "1234", profil: "Root",   societeId: 1, headerName: "3CTEC" ,       headerValue: "demo2025");
            await CheckUsersAsync(nom: "Francois Pascaud",  email: "francois@stratberries.com",   password: "1234", profil: "Root",   societeId: 2, headerName: "Stratberries", headerValue: "demo2025");
            await CheckUsersAsync(nom: "Christophe CAPES",  email: "christophe.capes@3ctec.fr",   password: "1234", profil: "Admin",  societeId: 1, headerName: "3CTEC",        headerValue: "demo2025");
            await CheckUsersAsync(nom: "Cloe",              email: "cloe@kropkontrol.com",        password: "1234", profil: "UserRW", societeId: 3, headerName: "Invenio",      headerValue: "28T3@3p2y$YjRC#Aii");
            await CheckUsersAsync(nom: "Stéphane Pozzobon", email: "stephane.pozzobon@gmail.com", password: "1234", profil: "UserRW", societeId: 4, headerName: "Pozzobon",     headerValue: "A8F3T3a2y$YjtB#AiB");        
            //await CriarTabelaDevicesModelsSeNaoExistirAsync(context);
            //await SeedDeviceModelsAsync(context);
        }

        private async Task<User> CheckUsersAsync(string nom, string email, string password, string profil, int societeId, string headerName, string headerValue)
        {
            var utilisateurExistant = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (utilisateurExistant != null)
            {
                return utilisateurExistant!;
            }

            User utilisateur = new()
            {
                Email = email,
                Nom = nom,
                Role = profil,
                Password = password,
                SignupDate = DateTime.UtcNow,
                CompanyId = societeId,
                HeaderName = headerName,
                HeaderValue = headerValue
            };

            utilisateur.Password = BCrypt.Net.BCrypt.HashPassword(utilisateur.Password);

            _context.Users.Add(utilisateur);
            await _context.SaveChangesAsync();
            return utilisateur;
        }
    }
}
