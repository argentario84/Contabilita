using System.ComponentModel.DataAnnotations;
using Contabilita.Core.Enums;

namespace Contabilita.API.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public string? Notes { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryColor { get; set; }
    public int? ScheduledExpenseId { get; set; }
}

public class CreateTransactionDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int? ScheduledExpenseId { get; set; }
}

public class UpdateTransactionDto
{
    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? Date { get; set; }

    public TransactionType? Type { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? CategoryId { get; set; }
}

public class TransactionSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public decimal CurrentBudget { get; set; }
    public List<CategorySummaryDto> ExpensesByCategory { get; set; } = new();
}

public class CategorySummaryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryColor { get; set; }
    public decimal Total { get; set; }
    public decimal Percentage { get; set; }
}
