using Contabilita.Core.Enums;

namespace Contabilita.Core.Entities;

public class Transaction : BaseEntity
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public string? Notes { get; set; }

    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public int? ScheduledExpenseId { get; set; }
    public virtual ScheduledExpense? ScheduledExpense { get; set; }
}
