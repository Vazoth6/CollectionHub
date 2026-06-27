using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Data.Model.DTOs
{
    // <summary>
    // DTO para criar um novo item
    // </summary>
    public class CreateItemDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome do Item")]
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        // <summary>
        // Obtém ou define descrição.
        // </summary>
        public string? Description { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, 999999.99, ErrorMessage = "O preço deve estar entre 0.01 e 999999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Preço")]
        // <summary>
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        [Display(Name = "Categoria")]
        // <summary>
        // Obtém ou define id da categoria.
        // </summary>
        public int CategoryId { get; set; }

        [Display(Name = "URL da Imagem")]
        // <summary>
        // Obtém ou define endereço da imagem.
        // </summary>
        public string? ImageUrl { get; set; }
    }

    // <summary>
    // DTO para actualizar um item existente
    // </summary>
    public class UpdateItemDto
    {
        [Required]
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        // <summary>
        // Obtém ou define descrição.
        // </summary>
        public string? Description { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, 999999.99)]
        // <summary>
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        // <summary>
        // Obtém ou define id da categoria.
        // </summary>
        public int CategoryId { get; set; }

        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string? Status { get; set; }
    }

    // <summary>
    // DTO para resposta de item (API - Cliente)
    // </summary>
    public class ItemResponseDto
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
        // Obtém ou define descrição.
        // </summary>
        public string? Description { get; set; }
        // <summary>
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }
        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string Status { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define endereço da imagem.
        // </summary>
        public string? ImageUrl { get; set; }
        // <summary>
        // Obtém ou define id da categoria.
        // </summary>
        public int CategoryId { get; set; }
        // <summary>
        // Obtém ou define nome da categoria.
        // </summary>
        public string CategoryName { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define nome do vendedor.
        // </summary>
        public string? SellerName { get; set; }
        // <summary>
        // Obtém ou define id do vendedor.
        // </summary>
        public int? SellerId { get; set; }
    }

    // <summary>
    // DTO para resposta de item do utilizador
    // </summary>
    public class MyItemResponseDto
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
        // Obtém ou define descrição.
        // </summary>
        public string? Description { get; set; }
        // <summary>
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }
        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string Status { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define id da categoria.
        // </summary>
        public int CategoryId { get; set; }
        // <summary>
        // Obtém ou define nome da categoria.
        // </summary>
        public string CategoryName { get; set; } = string.Empty;
    }

    // <summary>
    // DTO para listagem de items com filtros
    // </summary>
    public class ItemListQueryDto
    {
        // <summary>
        // Obtém ou define search term.
        // </summary>
        public string? SearchTerm { get; set; }
        // <summary>
        // Obtém ou define id da categoria.
        // </summary>
        public int? CategoryId { get; set; }
        // <summary>
        // Obtém ou define categorias selecionadas.
        // </summary>
        public List<string>? SelectedCategories { get; set; }
        // <summary>
        // Obtém ou define preço mínimo.
        // </summary>
        public decimal? MinPrice { get; set; }
        // <summary>
        // Obtém ou define preço máximo.
        // </summary>
        public decimal? MaxPrice { get; set; }
        // <summary>
        // Obtém ou define sort by.
        // </summary>
        public string? SortBy { get; set; }
        // <summary>
        // Obtém ou define página.
        // </summary>
        public int Page { get; set; } = 1;
        // <summary>
        // Obtém ou define tamanho da página.
        // </summary>
        public int PageSize { get; set; } = 12;
    }
}
