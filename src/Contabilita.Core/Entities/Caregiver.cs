namespace Contabilita.Core.Entities;

public class Caregiver : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Relationship { get; set; } // Nonno, Nonna, Zio, Zia, etc.
    public string? Color { get; set; }
    public string? Phone { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Condiviso tra tutti gli utenti della famiglia
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual ICollection<ChildcareSlot> ChildcareSlots { get; set; } = new List<ChildcareSlot>();
}
