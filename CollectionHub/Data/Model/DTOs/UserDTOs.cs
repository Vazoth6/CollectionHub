using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Data.Model.DTOs
{
    // <summary>
    // DTO para actualizar o perfil do utilizador
    // </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Telemóvel")]
        [StringLength(20)]
        [RegularExpression(@"^(\+[0-9]{1,3})?[0-9]{9,12}$",
            ErrorMessage = "Número de telemóvel inválido")]
        // <summary>
        // Obtém ou define número de telemóvel.
        // </summary>
        public string? CellPhone { get; set; }
    }

    // <summary>
    // DTO para actualizar o cargo de um utilizador
    // </summary>
    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "O cargo é obrigatório")]
        [Display(Name = "Cargo")]
        // <summary>
        // Obtém ou define perfil.
        // </summary>
        public string Role { get; set; } = string.Empty;
    }

    // <summary>
    // DTO para resposta de perfil do utilizador
    // </summary>
    public class UserProfileResponseDto
    {
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define número de telemóvel.
        // </summary>
        public string? CellPhone { get; set; }
        // <summary>
        // Obtém ou define perfil.
        // </summary>
        public string Role { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define data de registo.
        // </summary>
        public DateTime RegisterDate { get; set; }
        // <summary>
        // Obtém ou define email.
        // </summary>
        public string Email { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define estatísticas.
        // </summary>
        public UserStatisticsDto Statistics { get; set; } = new();
    }

    // <summary>
    // DTO para estatísticas do utilizador
    // </summary>
    public class UserStatisticsDto
    {
        // <summary>
        // Obtém ou define items para venda.
        // </summary>
        public int ItemsForSale { get; set; }
        // <summary>
        // Obtém ou define vendas concluídas.
        // </summary>
        public int CompletedSales { get; set; }
        // <summary>
        // Obtém ou define compras concluídas.
        // </summary>
        public int CompletedPurchases { get; set; }
        // <summary>
        // Obtém ou define receita total.
        // </summary>
        public decimal TotalRevenue { get; set; }
    }

    // <summary>
    // DTO para listagem de utilizadores (Admin)
    // </summary>
    public class UserListItemDto
    {
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define número de telemóvel.
        // </summary>
        public string? CellPhone { get; set; }
        // <summary>
        // Obtém ou define perfil.
        // </summary>
        public string Role { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define data de registo.
        // </summary>
        public DateTime RegisterDate { get; set; }
        // <summary>
        // Obtém ou define user id.
        // </summary>
        public string UserID { get; set; } = string.Empty;
    }

    // <summary>
    // DTO para resposta de vendedor
    // </summary>
    public class SellerResponseDto
    {
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define items para venda.
        // </summary>
        public int ItemsForSale { get; set; }
    }
}
