using System.ComponentModel.DataAnnotations;
using Contabilita.Core.Enums;

namespace Contabilita.API.DTOs;

public class ScheduledExpenseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public RecurrenceType Recurrence { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime NextDueDate { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryColor { get; set; }
    public bool IsDueToday { get; set; }
}

public class CreateScheduledExpenseDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public RecurrenceType Recurrence { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public int CategoryId { get; set; }
}

public class UpdateScheduledExpenseDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public RecurrenceType? Recurrence { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }

    public int? CategoryId { get; set; }
}

public class ConfirmScheduledExpenseDto
{
    public decimal? ActualAmount { get; set; }
    public string? Notes { get; set; }
}
