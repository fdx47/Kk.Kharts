using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kk.Kharts.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CacheVersionController : ControllerBase
    {
        private readonly ICacheVersionService _cacheVersionService;

        public CacheVersionController(ICacheVersionService cacheVersionService)
        {
            _cacheVersionService = cacheVersionService;
        }

        /// <summary>
        /// Obtient les versions actuelles du cache (localStorage, indexedDB, cache)
        /// Le frontend utilise ceci pour vérifier s'il doit nettoyer le cache
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<CacheVersionDTO>> GetCacheVersions()
        {
            var result = await _cacheVersionService.GetCurrentVersionsAsync();
            return Ok(result);
        }


        /// <summary>
        /// Incrémente les versions du cache
        /// - Sans paramètres: Incrémente toutes les versions (+1)
        /// - Avec paramètres: Incrémente uniquement les versions spécifiées
        /// Seul Root peut mettre à jour
        /// </summary>
        [HttpPost("increment")]
        [Authorize(Roles = "Root")]
        public async Task<ActionResult<CacheVersionDTO>> IncrementCacheVersions([FromBody] UpdateCacheVersionDTO? dto = null)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            var result = await _cacheVersionService.IncrementVersionsAsync(dto, userName);
            return Ok(result);
        }


        /// <summary>
        /// Définit des valeurs spécifiques pour les versions du cache
        /// Seul Root peut mettre à jour
        /// </summary>
        [HttpPut("set")]
        [Authorize(Roles = "Root")]
        public async Task<ActionResult<CacheVersionDTO>> SetCacheVersions([FromBody] SetCacheVersionDTO dto)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            var result = await _cacheVersionService.SetVersionsAsync(dto, userName);
            return Ok(result);
        }
    }
}
