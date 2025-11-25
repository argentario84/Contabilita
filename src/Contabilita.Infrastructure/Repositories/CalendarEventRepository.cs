using Microsoft.EntityFrameworkCore;
using Contabilita.Core.Entities;
using Contabilita.Core.Interfaces;
using Contabilita.Infrastructure.Data;

namespace Contabilita.Infrastructure.Repositories;

public class CalendarEventRepository : Repository<CalendarEvent>, ICalendarEventRepository
{
    public CalendarEventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CalendarEvent>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CalendarEvent>> GetByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(e => e.UserId == userId &&
                       ((e.StartDate >= startDate && e.StartDate <= endDate) ||
                        (e.EndDate.HasValue && e.EndDate >= startDate && e.EndDate <= endDate)))
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }
}
