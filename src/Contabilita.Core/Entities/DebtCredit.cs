using Contabilita.Core.Enums;

namespace Contabilita.Core.Entities;

public class DebtCredit : BaseEntity
{
    public DebtCreditType Type { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsSettled { get; set; } = false;
    public DateTime? SettledAt { get; set; }
    public string? Notes { get; set; }

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
}
