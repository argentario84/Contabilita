using Microsoft.EntityFrameworkCore;
using Contabilita.Core.Entities;
using Contabilita.Core.Interfaces;
using Contabilita.Infrastructure.Data;

namespace Contabilita.Infrastructure.Repositories;

public class ChildcareSlotRepository : Repository<ChildcareSlot>, IChildcareSlotRepository
{
    public ChildcareSlotRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ChildcareSlot>> GetByUserIdAndWeekAsync(string userId, DateTime weekStartDate)
    {
        return await _dbSet
            .Include(s => s.Caregiver)
            .Where(s => s.UserId == userId && s.WeekStartDate == weekStartDate.Date)
            .ToListAsync();
    }

    public async Task<ChildcareSlot?> GetBySlotAsync(string userId, DateTime weekStartDate, DayOfWeekEnum dayOfWeek, TimeSlot timeSlot)
    {
        return await _dbSet
            .Include(s => s.Caregiver)
            .FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.WeekStartDate == weekStartDate.Date &&
                s.DayOfWeek == dayOfWeek &&
                s.TimeSlot == timeSlot);
    }

    public async Task DeleteByWeekAsync(string userId, DateTime weekStartDate)
    {
        var slots = await _dbSet
            .Where(s => s.UserId == userId && s.WeekStartDate == weekStartDate.Date)
            .ToListAsync();

        _dbSet.RemoveRange(slots);
    }
}
