using Microsoft.EntityFrameworkCore;
using Contabilita.Core.Entities;
using Contabilita.Core.Interfaces;
using Contabilita.Infrastructure.Data;

namespace Contabilita.Infrastructure.Repositories;

public class CaregiverRepository : Repository<Caregiver>, ICaregiverRepository
{
    public CaregiverRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Caregiver>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Caregiver>> GetActiveByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }
}
