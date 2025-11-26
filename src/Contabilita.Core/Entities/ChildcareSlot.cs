namespace Contabilita.Core.Entities;

public enum DayOfWeekEnum
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7
}

public enum TimeSlot
{
    Morning = 1,    // Mattina
    Afternoon = 2,  // Pomeriggio
    Evening = 3     // Sera
}

public class ChildcareSlot : BaseEntity
{
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeSlot TimeSlot { get; set; }

    // Data specifica della settimana (per storico)
    public DateTime WeekStartDate { get; set; }

    public int CaregiverId { get; set; }
    public virtual Caregiver Caregiver { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
}
