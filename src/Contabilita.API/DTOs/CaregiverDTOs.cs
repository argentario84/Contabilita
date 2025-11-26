using System.ComponentModel.DataAnnotations;

namespace Contabilita.API.DTOs;

public class CaregiverDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? Color { get; set; }
    public string? Phone { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCaregiverDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Relationship { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public int DisplayOrder { get; set; }
}

public class UpdateCaregiverDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? Relationship { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}
