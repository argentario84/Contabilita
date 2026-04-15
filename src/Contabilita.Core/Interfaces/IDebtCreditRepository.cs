using Contabilita.Core.Entities;

namespace Contabilita.Core.Interfaces;

public interface IDebtCreditRepository : IRepository<DebtCredit>
{
    Task<IEnumerable<DebtCredit>> GetByUserIdAsync(string userId);
}
