using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Data.Model.DTOs
{
    /// <summary>
    /// DTO para atualizar o perfil do utilizador
    /// </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Telemóvel")]
        [StringLength(20)]
        [RegularExpression(@"^(\+[0-9]{1,3})?[0-9]{9,12}$",
            ErrorMessage = "Número de telemóvel inválido")]
        public string? CellPhone { get; set; }
    }

    /// <summary>
    /// DTO para atualizar o role de um utilizador
    /// </summary>
    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "O role é obrigatório")]
        [Display(Name = "Cargo")]
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resposta de perfil do utilizador
    /// </summary>
    public class UserProfileResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CellPhone { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime RegisterDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public UserStatisticsDto Statistics { get; set; } = new();
    }

    /// <summary>
    /// DTO para estatísticas do utilizador
    /// </summary>
    public class UserStatisticsDto
    {
        public int ItemsForSale { get; set; }
        public int CompletedSales { get; set; }
        public int CompletedPurchases { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>
    /// DTO para listagem de utilizadores (Admin)
    /// </summary>
    public class UserListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CellPhone { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime RegisterDate { get; set; }
        public string UserID { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resposta de vendedor
    /// </summary>
    public class SellerResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ItemsForSale { get; set; }
    }
}