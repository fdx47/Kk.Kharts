using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kk.Kharts.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string ErrorMessage, UserDTO User)> CreateUserAsync(UserCreateDTO dto)
        {
            var validRoles = new List<string>
        {
            Roles.Root,
            Roles.SuperAdmin,
            Roles.Admin,
            Roles.UserRW,
            Roles.User,
            Roles.Technician,
            Roles.DemoRandom,
            Roles.Demo
        };

            if (!validRoles.Contains(dto.Role))
                return (false, "Le rôle spécifié n'est pas valide.", null!);

            if (dto.Role == Roles.Demo || dto.Role == Roles.DemoRandom)
            {
                var match = Regex.Match(dto.Email, @"^[a-zA-Z0-9]+_(\d{6})@kkdemo\.com$");
                if (!match.Success)
                {
                    return (false, "Invalid demo email format. Expected: username_ddMMyy@kkdemo.com", null!);
                }

                var datePart = match.Groups[1].Value;

                if (!DateTime.TryParseExact(datePart, "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime emailDate))
                {
                    return (false, "Invalid demo email format.", null!);
                }

                var today = DateTime.UtcNow.Date;

                if (emailDate.Date < today)
                {
                    return (false, $"Access expired. This demo email is valid only for {emailDate:dd/MM/yyyy}.", null!);
                }

                // Se passou daqui, a data é igual ou futura => permite acesso


            }


            if (!IsValidPassword(dto.Password))
            {
                return (false, "Le mot de passe doit comporter au moins 8 caractères, dont une majuscule, une minuscule, un chiffre et un caractère spécial.", null!);
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return (false, $"Cet e-mail est déjà associé à un compte... \n\n - {existingUser.Nom}\n - {existingUser.Email}", null!);

            var company = await _context.Companies.FindAsync(dto.CompanyId);
            if (company == null)
                return (false, "La société spécifiée n'existe pas.", null!);

            var utilisateur = new User
            {
                Nom = dto.Nom,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                SignupDate = DateTime.UtcNow,
                CompanyId = dto.CompanyId
            };

            _context.Users.Add(utilisateur);
            await _context.SaveChangesAsync();

            var userDto = new UserDTO
            {
                Id = utilisateur.Id,
                Nom = utilisateur.Nom,
                Email = utilisateur.Email,
                Role = utilisateur.Role,
                DateEnregistrement = utilisateur.SignupDate
            };

            return (true, null!, userDto);
        }

        private bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task UpdateUserAuthDataAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
