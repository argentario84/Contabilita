using Microsoft.AspNetCore.Identity;

namespace Contabilita.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal InitialBudget { get; set; }
    public decimal MonthlyIncome { get; set; }

    // Budget Planning
    public decimal? SavingsGoalAmount { get; set; }
    public decimal? SavingsGoalPercentage { get; set; }
    public bool UseSavingsPercentage { get; set; }
    public decimal? ExtraFixedExpenses { get; set; }
    public decimal BudgetAlertThreshold { get; set; } = 80;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<ScheduledExpense> ScheduledExpenses { get; set; } = new List<ScheduledExpense>();
    public virtual ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
    public virtual ICollection<Caregiver> Caregivers { get; set; } = new List<Caregiver>();
    public virtual ICollection<ChildcareSlot> ChildcareSlots { get; set; } = new List<ChildcareSlot>();
}
