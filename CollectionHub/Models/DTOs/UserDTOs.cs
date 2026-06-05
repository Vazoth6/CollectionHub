using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Models.DTOs;

public class UserReadDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? CellPhone { get; set; }
    public DateTime RegisterDate { get; set; }
    public string UserID { get; set; } = string.Empty;
}

public class UserCreateDTO
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Utilizador";

    [StringLength(20)]
    [Phone]
    public string? CellPhone { get; set; }

    [StringLength(450)]
    public string UserID { get; set; } = string.Empty;
}

public class UserUpdateDTO : UserCreateDTO
{
}
