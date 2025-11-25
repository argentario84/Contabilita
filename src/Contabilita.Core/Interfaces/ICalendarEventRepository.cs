using Contabilita.Core.Entities;

namespace Contabilita.Core.Interfaces;

public interface ICalendarEventRepository : IRepository<CalendarEvent>
{
    Task<IEnumerable<CalendarEvent>> GetByUserIdAsync(string userId);
    Task<IEnumerable<CalendarEvent>> GetByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
}
