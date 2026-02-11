using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers
{
    /// <summary>
    /// Gestion des entreprises (sociétés) dans le système KropKontrol.
    /// </summary>
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IUserContext _userContext;
        private readonly IDeviceService _deviceService;
        private readonly IHashIdService _hashIdService;

        public CompanyController(ICompanyService companyService, IUserContext userContext, IDeviceService deviceService, IHashIdService hashIdService)
        {
            _companyService = companyService;
            _userContext = userContext;
            _deviceService = deviceService;
            _hashIdService = hashIdService;
        }


        /// <summary>
        /// Récupère la liste de toutes les entreprises. Réservé aux administrateurs Root.
        /// </summary>
        /// <returns>Liste de toutes les entreprises enregistrées.</returns>
        /// <response code="200">Liste des entreprises récupérée avec succès.</response>
        [Authorize(Roles = "Root")]
        [HttpGet("api/v1/companies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        /// <summary>
        /// Récupère les détails d'une entreprise par son ID numérique.
        /// </summary>
        /// <param name="id">L'identifiant numérique de l'entreprise.</param>
        /// <returns>Les détails de l'entreprise.</returns>
        [Obsolete("Ce point de terminaison est obsolète. Utilisez GET /api/v2/companies/{hash} avec le PublicId.")]
        [HttpGet("api/v1/companies/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            var company = await _companyService.GetCompanyByIdAsync(id, authenticatedUser);
            if (company == null)
                return NotFound(new { message = "Entreprise non trouvée." });

            return Ok(company);
        }

        /// <summary>
        /// Récupère les détails d'une entreprise par son PublicId hashé.
        /// </summary>
        /// <param name="hash">Le PublicId hashé de l'entreprise.</param>
        /// <returns>Les détails de l'entreprise.</returns>
        [HttpGet("api/v1/companies/{hash}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByHash(string hash)
        {
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            var numericId = _hashIdService.Decode(hash);
            if (!numericId.HasValue)
                return BadRequest(new { message = "Identifiant hashé invalide." });

            var company = await _companyService.GetCompanyByIdAsync(numericId.Value, authenticatedUser);
            if (company == null)
                return NotFound(new { message = "Entreprise non trouvée." });

            return Ok(company);
        }

        /// <summary>
        /// Crée une nouvelle entreprise. Réservé aux administrateurs Root.
        /// </summary>
        /// <param name="dto">Les informations de la nouvelle entreprise.</param>
        /// <returns>L'entreprise créée avec son identifiant.</returns>
        /// <response code="201">Entreprise créée avec succès.</response>
        /// <response code="400">Données invalides.</response>
        [Authorize(Roles = "Root")]
        [HttpPost("api/v1/companies")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CompanyCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _companyService.CreateCompanyAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Met à jour une entreprise par ID numérique.
        /// </summary>
        [Obsolete("Ce point de terminaison est obsolète. Utilisez PUT /api/v2/companies/{hash} avec le PublicId.")]
        [Authorize(Roles = "Root")]
        [HttpPut("api/v1/companies/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _companyService.UpdateCompanyAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = "Entreprise non trouvée pour mise à jour." });

            return Ok(updated);
        }

        /// <summary>
        /// Met à jour une entreprise par PublicId hashé.
        /// </summary>
        [Authorize(Roles = "Root")]
        [HttpPut("api/v1/companies/{hash}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateByHash(string hash, [FromBody] CompanyUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var numericId = _hashIdService.Decode(hash);
            if (!numericId.HasValue)
                return BadRequest(new { message = "Identifiant hashé invalide." });

            var updated = await _companyService.UpdateCompanyAsync(numericId.Value, dto);
            if (updated == null)
                return NotFound(new { message = "Entreprise non trouvée pour mise à jour." });

            return Ok(updated);
        }


        /// <summary>
        /// Désactive une entreprise par ID numérique.
        /// </summary>
        [Obsolete("Ce point de terminaison est obsolète. Utilisez DELETE /api/v2/companies/{hash} avec le PublicId.")]
        [Authorize(Roles = "Root")]
        [HttpDelete("api/v1/companies/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _companyService.DisableCompanyAsync(id);

            if (!success)
                return NotFound(new { message = "Entreprise non trouvée pour suppression." });

            return NoContent();
        }

        /// <summary>
        /// Désactive une entreprise par PublicId hashé.
        /// </summary>
        [Authorize(Roles = "Root")]
        [HttpDelete("api/v1/companies/{hash}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteByHash(string hash)
        {
            var numericId = _hashIdService.Decode(hash);
            if (!numericId.HasValue)
                return BadRequest(new { message = "Identifiant hashé invalide." });

            var success = await _companyService.DisableCompanyAsync(numericId.Value);

            if (!success)
                return NotFound(new { message = "Entreprise non trouvée pour suppression." });

            return NoContent();
        }


    }
}
