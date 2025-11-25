using Contabilita.Core.Interfaces;
using Contabilita.Infrastructure.Data;

namespace Contabilita.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private ICategoryRepository? _categories;
    private ITransactionRepository? _transactions;
    private IScheduledExpenseRepository? _scheduledExpenses;
    private ICalendarEventRepository? _calendarEvents;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_context);

    public ITransactionRepository Transactions =>
        _transactions ??= new TransactionRepository(_context);

    public IScheduledExpenseRepository ScheduledExpenses =>
        _scheduledExpenses ??= new ScheduledExpenseRepository(_context);

    public ICalendarEventRepository CalendarEvents =>
        _calendarEvents ??= new CalendarEventRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
