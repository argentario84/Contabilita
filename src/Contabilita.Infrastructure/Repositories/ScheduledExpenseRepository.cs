using Microsoft.EntityFrameworkCore;
using Contabilita.Core.Entities;
using Contabilita.Core.Interfaces;
using Contabilita.Infrastructure.Data;

namespace Contabilita.Infrastructure.Repositories;

public class ScheduledExpenseRepository : Repository<ScheduledExpense>, IScheduledExpenseRepository
{
    public ScheduledExpenseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ScheduledExpense>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(s => s.Category)
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.NextDueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScheduledExpense>> GetActiveByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(s => s.Category)
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderBy(s => s.NextDueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScheduledExpense>> GetDueExpensesAsync(string userId, DateTime date)
    {
        return await _dbSet
            .Include(s => s.Category)
            .Where(s => s.UserId == userId && s.IsActive && s.NextDueDate.Date <= date.Date)
            .OrderBy(s => s.NextDueDate)
            .ToListAsync();
    }

    public async Task<ScheduledExpense?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Category)
            .Include(s => s.ConfirmedTransactions)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
