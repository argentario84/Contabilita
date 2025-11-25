using Microsoft.EntityFrameworkCore;
using Contabilita.Core.Entities;
using Contabilita.Core.Enums;
using Contabilita.Core.Interfaces;
using Contabilita.Infrastructure.Data;

namespace Contabilita.Infrastructure.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAndCategoryAsync(string userId, int categoryId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.CategoryId == categoryId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAndTypeAsync(string userId, TransactionType type)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Type == type)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalByUserIdAndTypeAsync(string userId, TransactionType type, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet.Where(t => t.UserId == userId && t.Type == type);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        return await query.SumAsync(t => t.Amount);
    }

    public async Task<Transaction?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Include(t => t.ScheduledExpense)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
