using Contabilita.Core.Entities;
using Contabilita.Core.Enums;

namespace Contabilita.Core.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Category>> GetByUserIdAndTypeAsync(string userId, TransactionType type);
}
