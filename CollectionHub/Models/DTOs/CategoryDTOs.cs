using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Models.DTOs;

public class CategoryReadDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CategoryCreateDTO
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class CategoryUpdateDTO : CategoryCreateDTO
{
}
