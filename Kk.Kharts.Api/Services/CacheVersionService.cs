using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Models;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services
{
    public class CacheVersionService : ICacheVersionService
    {
        private readonly AppDbContext _context;

        public CacheVersionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CacheVersionDTO> GetCurrentVersionsAsync()
        {
            var cacheVersion = await _context.CacheVersions
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (cacheVersion == null)
            {
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

            return MapToDto(cacheVersion);
        }

        public async Task<CacheVersionDTO> IncrementVersionsAsync(UpdateCacheVersionDTO? dto, string updatedBy)
        {
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
                    UpdatedBy = updatedBy
                };
                _context.CacheVersions.Add(currentVersion);
            }
            else
            {
                if (dto == null || (!dto.IncrementLocalStorage.HasValue && !dto.IncrementIndexedDb.HasValue && !dto.IncrementCache.HasValue))
                {
                    currentVersion.LocalStorageVersion++;
                    currentVersion.IndexedDbVersion++;
                    currentVersion.CacheStorageVersion++;
                }
                else
                {
                    if (dto.IncrementLocalStorage == true)
                        currentVersion.LocalStorageVersion++;

                    if (dto.IncrementIndexedDb == true)
                        currentVersion.IndexedDbVersion++;

                    if (dto.IncrementCache == true)
                        currentVersion.CacheStorageVersion++;
                }

                currentVersion.UpdatedAt = DateTime.UtcNow;
                currentVersion.UpdatedBy = updatedBy;
            }

            await _context.SaveChangesAsync();
            return MapToDto(currentVersion);
        }

        public async Task<CacheVersionDTO> SetVersionsAsync(SetCacheVersionDTO dto, string updatedBy)
        {
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
                    UpdatedBy = updatedBy
                };
                _context.CacheVersions.Add(currentVersion);
            }
            else
            {
                if (dto.LocalStorageVersion.HasValue)
                    currentVersion.LocalStorageVersion = dto.LocalStorageVersion.Value;

                if (dto.IndexedDbVersion.HasValue)
                    currentVersion.IndexedDbVersion = dto.IndexedDbVersion.Value;

                if (dto.CacheStorageVersion.HasValue)
                    currentVersion.CacheStorageVersion = dto.CacheStorageVersion.Value;

                currentVersion.UpdatedAt = DateTime.UtcNow;
                currentVersion.UpdatedBy = updatedBy;
            }

            await _context.SaveChangesAsync();
            return MapToDto(currentVersion);
        }

        private static CacheVersionDTO MapToDto(CacheVersion v) => new()
        {
            LocalStorageVersion = v.LocalStorageVersion,
            IndexedDbVersion = v.IndexedDbVersion,
            CacheStorageVersion = v.CacheStorageVersion,
            UpdatedAt = v.UpdatedAt
        };
    }
}
