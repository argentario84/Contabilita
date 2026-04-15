using System.ComponentModel.DataAnnotations;
using Contabilita.Core.Enums;

namespace Contabilita.API.DTOs;

public class DebtCreditDto
{
    public int Id { get; set; }
    public DebtCreditType Type { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsSettled { get; set; }
    public DateTime? SettledAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsOverdue { get; set; }
    public int? DaysUntilDue { get; set; }
}

public class CreateDebtCreditDto
{
    [Required]
    public DebtCreditType Type { get; set; }

    [Required]
    [MaxLength(200)]
    public string PersonName { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class UpdateDebtCreditDto
{
    public DebtCreditType? Type { get; set; }

    [MaxLength(200)]
    public string? PersonName { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
