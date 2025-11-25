using Contabilita.Core.Entities;

namespace Contabilita.Core.Interfaces;

public interface IScheduledExpenseRepository : IRepository<ScheduledExpense>
{
    Task<IEnumerable<ScheduledExpense>> GetByUserIdAsync(string userId);
    Task<IEnumerable<ScheduledExpense>> GetActiveByUserIdAsync(string userId);
    Task<IEnumerable<ScheduledExpense>> GetDueExpensesAsync(string userId, DateTime date);
    Task<ScheduledExpense?> GetByIdWithDetailsAsync(int id);
}
