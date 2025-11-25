using Contabilita.Core.Enums;

namespace Contabilita.Core.Entities;

public class ScheduledExpense : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public RecurrenceType Recurrence { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime NextDueDate { get; set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual ICollection<Transaction> ConfirmedTransactions { get; set; } = new List<Transaction>();
}
