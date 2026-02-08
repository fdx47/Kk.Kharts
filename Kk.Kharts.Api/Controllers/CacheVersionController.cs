using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Models;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kk.Kharts.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CacheVersionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CacheVersionController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtient les versions actuelles du cache (localStorage, indexedDB, cache)
        /// Le frontend utilise ceci pour vérifier s'il doit nettoyer le cache
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<CacheVersionDTO>> GetCacheVersions()
        {
            var cacheVersion = await _context.CacheVersions
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (cacheVersion == null)
            {
                // Se não existir, cria um registro inicial
                cacheVersion = new CacheVersion
                {
                    LocalStorageVersion = 1,
                    IndexedDbVersion = 1,
                    CacheStorageVersion = 1,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                };

                _context.CacheVersions.Add(cacheVersion);
                await _context.SaveChangesAsync();
            }

            var dto = new CacheVersionDTO
            {
                LocalStorageVersion = cacheVersion.LocalStorageVersion,
                IndexedDbVersion = cacheVersion.IndexedDbVersion,
                CacheStorageVersion = cacheVersion.CacheStorageVersion,
                UpdatedAt = cacheVersion.UpdatedAt
            };

            return Ok(dto);
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

            var currentVersion = await _context.CacheVersions
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (currentVersion == null)
            {
                currentVersion = new CacheVersion
                {
                    LocalStorageVersion = 1,
                    IndexedDbVersion = 1,
                    CacheStorageVersion = 1,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = userName
                };
                _context.CacheVersions.Add(currentVersion);
            }
            else
            {
                // Se dto for null ou sem valores, incrementa tudo (+1)
                if (dto == null || (!dto.IncrementLocalStorage.HasValue && !dto.IncrementIndexedDb.HasValue && !dto.IncrementCache.HasValue))
                {
                    currentVersion.LocalStorageVersion++;
                    currentVersion.IndexedDbVersion++;
                    currentVersion.CacheStorageVersion++;
                }
                else
                {
                    // Incrementa apenas as versões especificadas (sempre +1)
                    if (dto.IncrementLocalStorage == true)
                    {
                        currentVersion.LocalStorageVersion++;
                    }

                    if (dto.IncrementIndexedDb == true)
                    {
                        currentVersion.IndexedDbVersion++;
                    }

                    if (dto.IncrementCache == true)
                    {
                        currentVersion.CacheStorageVersion++;
                    }
                }

                currentVersion.UpdatedAt = DateTime.UtcNow;
                currentVersion.UpdatedBy = userName;
            }

            await _context.SaveChangesAsync();

            var result = new CacheVersionDTO
            {
                LocalStorageVersion = currentVersion.LocalStorageVersion,
                IndexedDbVersion = currentVersion.IndexedDbVersion,
                CacheStorageVersion = currentVersion.CacheStorageVersion,
                UpdatedAt = currentVersion.UpdatedAt
            };

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

            var currentVersion = await _context.CacheVersions
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (currentVersion == null)
            {
                currentVersion = new CacheVersion
                {
                    LocalStorageVersion = dto.LocalStorageVersion ?? 1,
                    IndexedDbVersion = dto.IndexedDbVersion ?? 1,
                    CacheStorageVersion = dto.CacheStorageVersion ?? 1,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = userName
                };
                _context.CacheVersions.Add(currentVersion);
            }
            else
            {
                // Define valores específicos apenas para as versões especificadas
                if (dto.LocalStorageVersion.HasValue)
                {
                    currentVersion.LocalStorageVersion = dto.LocalStorageVersion.Value;
                }

                if (dto.IndexedDbVersion.HasValue)
                {
                    currentVersion.IndexedDbVersion = dto.IndexedDbVersion.Value;
                }

                if (dto.CacheStorageVersion.HasValue)
                {
                    currentVersion.CacheStorageVersion = dto.CacheStorageVersion.Value;
                }

                currentVersion.UpdatedAt = DateTime.UtcNow;
                currentVersion.UpdatedBy = userName;
            }

            await _context.SaveChangesAsync();

            var result = new CacheVersionDTO
            {
                LocalStorageVersion = currentVersion.LocalStorageVersion,
                IndexedDbVersion = currentVersion.IndexedDbVersion,
                CacheStorageVersion = currentVersion.CacheStorageVersion,
                UpdatedAt = currentVersion.UpdatedAt
            };

            return Ok(result);
        }
    }
}
