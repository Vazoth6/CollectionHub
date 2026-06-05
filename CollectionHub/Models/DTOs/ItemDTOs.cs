using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Models.DTOs;

public class ItemReadDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

public class ItemCreateDTO
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    public string Status { get; set; } = "Disponível";

    public string? ImageUrl { get; set; }

    [Required]
    public int CategoryId { get; set; }
}

public class ItemUpdateDTO : ItemCreateDTO
{
}
