using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Data.Model.DTOs
{
    // <summary>
    // DTO para criar uma nova categoria
    // </summary>
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "O nome da categoria é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome da Categoria")]
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;
    }

    // <summary>
    // DTO para actualizar uma categoria existente
    // </summary>
    public class UpdateCategoryDto
    {
        [Required]
        // <summary>
        // Obtém ou define identificador.
        // </summary>
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório")]
        [StringLength(100)]
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;
    }

    // <summary>
    // DTO para resposta de categoria (com contagem de items)
    // </summary>
    public class CategoryResponseDto
    {
        // <summary>
        // Obtém ou define identificador.
        // </summary>
        public int Id { get; set; }
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define item count.
        // </summary>
        public int ItemCount { get; set; }
    }

    // <summary>
    // DTO para categoria com detalhes dos items
    // </summary>
    public class CategoryDetailResponseDto
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
        // Obtém ou define artigos.
        // </summary>
        public List<CategoryItemDto> Items { get; set; } = new();
    }

    // <summary>
    // Representa os dados transferidos entre a interface/API e a aplicação para category item dto.
    // </summary>
    public class CategoryItemDto
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
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }
        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string Status { get; set; } = string.Empty;
    }
}
