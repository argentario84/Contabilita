using Contabilita.Core.Entities;

namespace Contabilita.Core.Interfaces;

public interface IChildcareSlotRepository : IRepository<ChildcareSlot>
{
    Task<IEnumerable<ChildcareSlot>> GetByUserIdAndWeekAsync(string userId, DateTime weekStartDate);
    Task<ChildcareSlot?> GetBySlotAsync(string userId, DateTime weekStartDate, DayOfWeekEnum dayOfWeek, TimeSlot timeSlot);
    Task DeleteByWeekAsync(string userId, DateTime weekStartDate);
}
