using Kk.Kharts.Api.Attributes;
using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.RegularExpressions;

namespace Kk.Kharts.Api.Controllers
{
    //[Route("api/v1/[controller]")]
    [Route("api/v1/users")]
    [ApiController]
    //[Authorize]

    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;

        public UsersController(AppDbContext context, IEmailService emailService, IUserService userService)
        {
            _context = context;
            _emailService = emailService;
            _userService = userService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var utilisateurs = await _context.Users.ToListAsync();

            // Mapeando para UserDTO
            var userDTOs = utilisateurs.Select(u => new UserDTO
            {
                Id = u.Id,
                Nom = u.Nom,
                Email = u.Email,
                Role = u.Role,
                DateEnregistrement = u.SignupDate
            }).ToList();

            return Ok(userDTOs);
        }


        [Authorize(Roles = "Root")] // Apenas 'Admin' pode acessar essa rota
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var utilisateur = await _context.Users.FindAsync(id);
            return Ok(utilisateur);
        }


        [Authorize(Roles = Roles.Root)]
        [HttpPost]
        public async Task<IActionResult> Post(UserCreateDTO dto)
        {
            var result = await _userService.CreateUserAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                message = "Utilisateur enregistré avec succès.",
                user = result.User
            });
        }

        //[Authorize(Roles = Roles.Root)]
        //[HttpPost]
        //public async Task<IActionResult> Post(UserCreateDTO dto)
        //{
        //    var validRoles = new List<string>
        //    {
        //        Roles.Root,
        //        Roles.SuperAdmin,
        //        Roles.Admin,
        //        Roles.UserRW,
        //        Roles.User,
        //        Roles.Technician,
        //        Roles.DemoRandom,
        //        Roles.Demo
        //    };

        //    if (!validRoles.Contains(dto.Role))
        //        return BadRequest(new { message = "Le rôle spécifié n'est pas valide." });

        //    // 🔐 Se o role for Demo ou DemoRandom, verifica formato do e-mail
        //    if ((dto.Role == Roles.Demo || dto.Role == Roles.DemoRandom) && !Regex.IsMatch(dto.Email, @"^\d{6}@kropkontrol\.com$"))
        //    {
        //        return BadRequest(new
        //        {
        //            message = "Pour les rôles Demo/DemoRandom, l’e-mail doit être au format ddmmyy@kropkontrol.com"
        //        });
        //    }

        //    if (!IsValidPassword(dto.Password))
        //        return BadRequest(new { message = "Le mot de passe doit comporter au moins 8 caractères, dont une majuscule, une minuscule, un chiffre et un caractère spécial."});

        //    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        //    if (existingUser != null)
        //        return BadRequest(new { message = "L’e-mail est déjà utilisé." });

        //    // Vérifie que la société existe
        //    var company = await _context.Companies.FindAsync(dto.CompanyId);
        //    if (company == null)
        //        return BadRequest(new { message = "La société spécifiée n'existe pas." });

        //    var utilisateur = new User
        //    {
        //        Nom = dto.Nom,
        //        Email = dto.Email,
        //        Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        //        Role = dto.Role,
        //        SignupDate = DateTime.UtcNow,
        //        CompanyId = dto.CompanyId 
        //    };

        //    _context.Users.Add(utilisateur);
        //    await _context.SaveChangesAsync();

        //    var userDto = new UserDTO
        //    {
        //        Id = utilisateur.Id,
        //        Nom = utilisateur.Nom,
        //        Email = utilisateur.Email,
        //        Role = utilisateur.Role,
        //        DateEnregistrement = utilisateur.SignupDate
        //    };

        //    return Ok(new
        //    {
        //        message = "Utilisateur enregistré avec succès.",
        //        user = userDto
        //    });
        //}




        [Authorize(Roles = Roles.Root)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, User usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest();
            }
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(usuario);
        }


        [Authorize(Roles = Roles.Root)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = new User { Id = id };
            _context.Users.Remove(usuario);
            await _context.SaveChangesAsync();
            return Ok(usuario);
        }


        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyAccount(UserUpdateSelfDTO dto)
        {
            //var userIdFdx = int.Parse(User.FindFirst("nameid")?.Value ?? "0");  
            var userId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? "0");

            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound("Utilisateur non trouvé.");

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {               
                user.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Vos informations ont été mises à jour avec succès." });
        }


        // Endpoint: Solicitar troca de e-mail
        [Authorize]
        [DenyAccessForRole(isWriteAccessRequired: true)]  // Exige role de leitura e escrita
        [HttpPost("request-email-change")]
        public async Task<IActionResult> RequestEmailChange([FromBody] string newEmail)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");

            // Verifica se já está em uso
            if (await _context.Users.AnyAsync(u => u.Email == newEmail))
                return BadRequest("Cette adresse e-mail est déjà utilisée.");

            // Cria token aleatório
            var token = Guid.NewGuid().ToString();

            var pending = new PendingEmailChange
            {
                UserId = userId,
                NewEmail = newEmail,
                Token = token,
                RequestedAt = DateTime.UtcNow
            };

            _context.PendingEmailChanges.Add(pending);
            await _context.SaveChangesAsync();

            var confirmLink = $"https://kropkontrol.premiumasp.net/api/v1/users/confirm-email-change?token={token}";

            // Aqui você envia o e-mail (use SMTP, SendGrid, etc.)
            await _emailService.SendAsync(newEmail, "Confirmez votre nouvelle adresse e-mail",
                $"Cliquez ici pour confirmer le changement : {confirmLink}");

            return Ok(new { message = "Un lien de confirmation a été envoyé à votre nouvelle adresse e-mail." });
        }


        //Endpoint: Confirmar a troca via link
        [HttpGet("confirm-email-change")]
        public async Task<IActionResult> ConfirmEmailChange([FromQuery] string token)
        {
            var pending = await _context.PendingEmailChanges
                .FirstOrDefaultAsync(p => p.Token == token && !p.Confirmed);

            if (pending == null || (DateTime.UtcNow - pending.RequestedAt).TotalHours > 24)
                return BadRequest("Le lien est invalide ou a expiré.");

            var user = await _context.Users.FindAsync(pending.UserId);
            if (user == null) return NotFound();

            user.Email = pending.NewEmail;
            pending.Confirmed = true;

            await _context.SaveChangesAsync();

            return Ok("Votre adresse e-mail a été mise à jour avec succès.");
        }

        [Authorize]
        [DenyAccessForRole(isWriteAccessRequired: true)]  // Exige role de leitura e escrita
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
        {
            //var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            //if (user == null)
            //    return Ok(); // Para evitar enumeração de e-mails

            var token = Guid.NewGuid().ToString();

            //var pending = new PendingPasswordReset
            //{
            //    UserId = user.Id,
            //    Token = token,
            //    RequestedAt = DateTime.UtcNow
            //};

            //_context.PendingPasswordResets.Add(pending);
            //await _context.SaveChangesAsync();

            email = "carnbq@gmail.com";

            var resetLink = $"https://kropkontrol.premiumasp.net/api/v1/Users/reset-password?token={token}";

            await _emailService.SendAsync(email, "Réinitialisation de mot de passe",
                $"Cliquez sur ce lien pour réinitialiser votre mot de passe : {resetLink}");

            return Ok(new { message = "Un lien de réinitialisation a été envoyé à votre adresse e-mail." });
        }


        [HttpPost("confirm-password-reset")]
        public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetDTO dto)
        {
            var pending = await _context.PendingPasswordResets
                .FirstOrDefaultAsync(p => p.Token == dto.Token && !p.Used);

            if (pending == null || (DateTime.UtcNow - pending.RequestedAt).TotalHours > 24)
                return BadRequest("Le lien est invalide ou a expiré.");

            var user = await _context.Users.FindAsync(pending.UserId);
            if (user == null) return NotFound("Utilisateur non trouvé.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            pending.Used = true;

            await _context.SaveChangesAsync();

            return Ok("Votre mot de passe a été modifié avec succès.");
        }


        private bool IsValidPassword(string password)
        {
            // Regex - au moins 8 caractères, dont une majuscule, une minuscule, un chiffre et un caractère spécial.
            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""{}|<>]).{8,}$");
            // Vérifie si la chaîne correspond à la regex
            return passwordRegex.IsMatch(password);
        }
    }
}