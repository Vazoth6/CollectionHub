using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Data.Model.DTOs
{
    /// <summary>
    /// DTO para criar uma nova categoria
    /// </summary>
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "O nome da categoria é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome da Categoria")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para atualizar uma categoria existente
    /// </summary>
    public class UpdateCategoryDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resposta de categoria (com contagem de itens)
    /// </summary>
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// DTO para categoria com detalhes dos itens
    /// </summary>
    public class CategoryDetailResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<CategoryItemDto> Items { get; set; } = new();
    }

    public class CategoryItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}