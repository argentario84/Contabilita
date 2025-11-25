using System.ComponentModel.DataAnnotations;
using Contabilita.Core.Enums;

namespace Contabilita.API.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public TransactionType Type { get; set; }
    public decimal? MonthlyBudget { get; set; }
    public decimal SpentThisMonth { get; set; }
    public decimal? RemainingBudget { get; set; }
    public decimal? BudgetPercentageUsed { get; set; }
}

public class CreateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MonthlyBudget { get; set; }
}

public class UpdateCategoryDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    public TransactionType? Type { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MonthlyBudget { get; set; }
}

public class CategoryBudgetSummaryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryColor { get; set; }
    public decimal? MonthlyBudget { get; set; }
    public decimal SpentThisMonth { get; set; }
    public decimal? RemainingBudget { get; set; }
    public decimal? BudgetPercentageUsed { get; set; }
    public bool IsOverBudget { get; set; }
}
