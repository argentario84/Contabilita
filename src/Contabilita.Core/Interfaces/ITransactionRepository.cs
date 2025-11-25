using Contabilita.Core.Entities;
using Contabilita.Core.Enums;

namespace Contabilita.Core.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Transaction>> GetByUserIdAndCategoryAsync(string userId, int categoryId);
    Task<IEnumerable<Transaction>> GetByUserIdAndTypeAsync(string userId, TransactionType type);
    Task<decimal> GetTotalByUserIdAndTypeAsync(string userId, TransactionType type, DateTime? startDate = null, DateTime? endDate = null);
    Task<Transaction?> GetByIdWithDetailsAsync(int id);
}
