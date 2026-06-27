using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    public class Item
    {
        /// <summary>
        /// PK
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome do item
        /// </summary>
        [Required]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição do item
        /// </summary>
        [StringLength(1000)]
        [Display(Name = "Descrição")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Preço do item
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        /// <summary>
        /// Estado do item
        /// </summary>
        [Display(Name = "Estado")]
        public string Status { get; set; } = "Disponível";

        /// <summary>
        /// URL ou Ficheiro da imagem do item
        /// </summary>
        [Display(Name = "Imagem")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Data de submissão do item
        /// </summary>
        [Display(Name = "Data de submissão")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Chaves estrangeiras (FK) para a categoria do item (Category)
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Categoria associada a este item.
        /// </summary>
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        /// <summary>
        /// Itens que pertencem a este utilizador (relação 1:N com UserItem)
        /// </summary>
        public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();

        /// <summary>
        /// Transações associadas a este item (relação 1:N com Transaction)
        /// </summary>
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        /// <summary>
        /// Gostos associados a este item (relação 1:N com ItemLike)
        /// </summary>
        public ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
    }
}