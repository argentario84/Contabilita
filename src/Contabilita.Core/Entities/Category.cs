using Contabilita.Core.Enums;

namespace Contabilita.Core.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public TransactionType Type { get; set; }
    public decimal? MonthlyBudget { get; set; }
    public bool RequireDescription { get; set; } = false;

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<ScheduledExpense> ScheduledExpenses { get; set; } = new List<ScheduledExpense>();
}
