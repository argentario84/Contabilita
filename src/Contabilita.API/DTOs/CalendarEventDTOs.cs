using System.ComponentModel.DataAnnotations;

namespace Contabilita.API.DTOs;

public class CalendarEventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool AllDay { get; set; }
    public string? Color { get; set; }
    public bool IsShared { get; set; }
    public string? CreatedByName { get; set; }
}

public class CreateCalendarEventDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool AllDay { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public bool IsShared { get; set; } = false;
}

public class UpdateCalendarEventDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? AllDay { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public bool? IsShared { get; set; }
}
