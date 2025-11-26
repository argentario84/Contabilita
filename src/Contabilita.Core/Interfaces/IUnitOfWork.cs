namespace Contabilita.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    ITransactionRepository Transactions { get; }
    IScheduledExpenseRepository ScheduledExpenses { get; }
    ICalendarEventRepository CalendarEvents { get; }
    ICaregiverRepository Caregivers { get; }
    IChildcareSlotRepository ChildcareSlots { get; }
    Task<int> SaveChangesAsync();
}
