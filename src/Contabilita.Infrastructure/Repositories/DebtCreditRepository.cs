using Microsoft.EntityFrameworkCore;
using Contabilita.Core.Entities;
using Contabilita.Core.Interfaces;
using Contabilita.Infrastructure.Data;

namespace Contabilita.Infrastructure.Repositories;

public class DebtCreditRepository : Repository<DebtCredit>, IDebtCreditRepository
{
    public DebtCreditRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DebtCredit>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.IsSettled)
            .ThenBy(d => d.DueDate.HasValue ? 0 : 1)
            .ThenBy(d => d.DueDate)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync();
    }
}
