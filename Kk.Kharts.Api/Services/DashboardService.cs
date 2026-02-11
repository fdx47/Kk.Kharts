using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dashboard?> GetByUserIdAsync(int userId)
        {
            return await _context.Dashboards
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<List<Dashboard>> GetAllByUserIdAsync(int userId)
        {
            return await _context.Dashboards
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveStateAsync(int userId, string stateJson)
        {
            var existing = await _context.Dashboards
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (existing != null)
            {
                existing.StateJson = stateJson;
                existing.CreatedAt = DateTime.UtcNow;
                _context.Dashboards.Update(existing);
            }
            else
            {
                _context.Dashboards.Add(new Dashboard
                {
                    StateJson = stateJson,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
