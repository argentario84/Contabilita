using Contabilita.Core.Entities;

namespace Contabilita.Core.Interfaces;

public interface ICaregiverRepository : IRepository<Caregiver>
{
    Task<IEnumerable<Caregiver>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Caregiver>> GetActiveByUserIdAsync(string userId);
}
