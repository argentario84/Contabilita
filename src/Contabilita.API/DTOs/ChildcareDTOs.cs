using System.ComponentModel.DataAnnotations;
using Contabilita.Core.Entities;

namespace Contabilita.API.DTOs;

public class ChildcareSlotDto
{
    public int Id { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeSlot TimeSlot { get; set; }
    public DateTime WeekStartDate { get; set; }
    public int CaregiverId { get; set; }
    public string CaregiverName { get; set; } = string.Empty;
    public string? CaregiverColor { get; set; }
}

public class CreateChildcareSlotDto
{
    [Required]
    public DayOfWeekEnum DayOfWeek { get; set; }

    [Required]
    public TimeSlot TimeSlot { get; set; }

    [Required]
    public DateTime WeekStartDate { get; set; }

    [Required]
    public int CaregiverId { get; set; }
}

public class UpdateChildcareSlotDto
{
    public int? CaregiverId { get; set; }
}

public class WeeklyChildcareDto
{
    public DateTime WeekStartDate { get; set; }
    public List<ChildcareSlotDto> Slots { get; set; } = new();
}

public class BulkUpdateChildcareDto
{
    [Required]
    public DateTime WeekStartDate { get; set; }

    public List<CreateChildcareSlotDto> Slots { get; set; } = new();
}
