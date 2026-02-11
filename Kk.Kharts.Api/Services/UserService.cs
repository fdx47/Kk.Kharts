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

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Nom = u.Nom,
                    Email = u.Email,
                    Role = u.Role,
                    DateEnregistrement = u.SignupDate
                })
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> UpdateUserAsync(int id, UserAdminUpdateDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Nom = dto.Nom;
            user.Email = dto.Email;
            user.Role = dto.Role;
            user.CompanyId = dto.CompanyId;

            if (dto.HeaderName != null)
                user.HeaderName = dto.HeaderName;
            if (dto.HeaderValue != null)
                user.HeaderValue = dto.HeaderValue;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = new User { Id = id };
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateSelfAccountAsync(int userId, UserUpdateSelfDTO dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                user.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsEmailInUseAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<string> CreateEmailChangeRequestAsync(int userId, string newEmail)
        {
            var token = Guid.NewGuid().ToString();

            _context.PendingEmailChanges.Add(new PendingEmailChange
            {
                UserId = userId,
                NewEmail = newEmail,
                Token = token,
                RequestedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<bool> ConfirmEmailChangeAsync(string token)
        {
            var pending = await _context.PendingEmailChanges
                .FirstOrDefaultAsync(p => p.Token == token && !p.Confirmed);

            if (pending == null || (DateTime.UtcNow - pending.RequestedAt).TotalHours > 24)
                return false;

            var user = await _context.Users.FindAsync(pending.UserId);
            if (user == null) return false;

            user.Email = pending.NewEmail;
            pending.Confirmed = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> CreatePasswordResetRequestAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            var token = Guid.NewGuid().ToString();

            _context.PendingPasswordResets.Add(new PendingPasswordReset
            {
                UserId = user.Id,
                Token = token,
                RequestedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<(bool Success, string? ErrorMessage)> ConfirmPasswordResetAsync(string token, string newPassword)
        {
            var pending = await _context.PendingPasswordResets
                .FirstOrDefaultAsync(p => p.Token == token && !p.Used);

            if (pending == null || (DateTime.UtcNow - pending.RequestedAt).TotalHours > 24)
                return (false, "Le lien est invalide ou a expiré.");

            var user = await _context.Users.FindAsync(pending.UserId);
            if (user == null) return (false, "Utilisateur non trouvé.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            pending.Used = true;

            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
